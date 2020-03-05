using Gdc.Scd.BusinessLogicLayer.Dto.Portfolio;
using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.CopyDataTool.Configuration;
using Gdc.Scd.CopyDataTool.Entities;
using Gdc.Scd.Core.Entities.Calculation;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Ninject;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using LocalPortfolio = Gdc.Scd.CopyDataTool.Entities.LocalPortfolio;
using TargetPortfolio = Gdc.Scd.Core.Entities.Portfolio;
using TE = Gdc.Scd.Core.Entities;

namespace Gdc.Scd.CopyDataTool
{
    public class ManualDataCopyService
    {
        private readonly IKernel kernel;
        private readonly CopyDetailsConfig config;
        private readonly Dictionary<string, Dictionary<string, long>> Dependencies;

        public ManualDataCopyService(IKernel kernel)
        {
            this.kernel = kernel;
            config = this.kernel.Get<CopyDetailsConfig>();
            Dependencies = new Dictionary<string, Dictionary<string, long>>();
            LoadDependencies();
        }

        public void CopyData()
        {
            if (config.CopyManualCosts)
            {
                Console.WriteLine("Manual costs coppying...");

                var userRepository = kernel.Get<IRepository<TE.User>>();
                var changedUser = userRepository.GetAll().FirstOrDefault(user => user.Login == config.EditUser);

                if (changedUser == null)
                    throw new Exception($"User {config.EditUser} could not be found in the target database");

                //UpdateStandardWarrantyManual(changedUser);
                UpdateHardwareManualCosts(changedUser);

                Console.WriteLine("Сopying completed");
            }
        }

        private void UpdateStandardWarrantyManual(TE.User editor)
        {

            using (var context = new SCD_2SourceEntities())
            {
                var sourceStandardWarrs = context.StandardWarrantyManualCost.Where(sw => sw.Country.Name == config.Country);

                if (!String.IsNullOrEmpty(config.ExcludedWgs))
                {
                    var excludedWgs = config.ExcludedWgs.Split(',');
                    sourceStandardWarrs = sourceStandardWarrs.Where(mc => !excludedWgs.Contains(mc.Wg.Name));
                }

                var targetWarrantyRepoSet = kernel.Get<IRepositorySet>();
                var targetWarrantyRepo = targetWarrantyRepoSet.GetRepository<TE.Calculation.StandardWarrantyManualCost>();
                var targetWarrantiesToUpdate = new List<TE.Calculation.StandardWarrantyManualCost>();

                foreach (var sourceStandardWarr in sourceStandardWarrs)
                {
                    var targetWarrToUpdate = targetWarrantyRepo.GetAll().FirstOrDefault(sw =>
                        sw.Wg.Name == sourceStandardWarr.Wg.Name &&
                        sw.Country.Name == sourceStandardWarr.Country.Name) ?? 
                                             new TE.Calculation.StandardWarrantyManualCost
                                             {
                                                 WgId = Dependencies[nameof(TE.Wg)][sourceStandardWarr.Wg.Name],
                                                 CountryId = Dependencies[nameof(TE.Country)][sourceStandardWarr.Country.Name]
                                             };

                    targetWarrToUpdate.ChangeDate = DateTime.Now;
                    targetWarrToUpdate.ChangeUser = editor;
                    targetWarrToUpdate.StandardWarranty = sourceStandardWarr.StandardWarranty;

                    targetWarrantiesToUpdate.Add(targetWarrToUpdate);
                }

                targetWarrantyRepo.Save(targetWarrantiesToUpdate);
                targetWarrantyRepoSet.Sync();
            }
        }

        private void UpdateHardwareManualCosts(TE.User editor)
        {
            using (var context = new SCD_2SourceEntities())
            {
                var sourceManualCosts = context.ManualCost.Include(mc => mc.LocalPortfolio)
                                        .Where(mc => mc.LocalPortfolio.Country.Name == config.Country);

                if (!String.IsNullOrEmpty(config.ExcludedWgs))
                {
                    var excludedWgs = config.ExcludedWgs.Split(',');
                    sourceManualCosts = sourceManualCosts.Where(mc => !excludedWgs.Contains(mc.LocalPortfolio.Wg.Name));
                }


                var sourcePortfolio = sourceManualCosts.Select(c => c.LocalPortfolio).ToList();

                
                var targetRepoSet = kernel.Get<IRepositorySet>();
                var targetManualRepo = targetRepoSet.GetRepository<HardwareManualCost>();
                var targetPortfolioRepo = targetRepoSet.GetRepository<TargetPortfolio.LocalPortfolio>();

                var targetManualCosts = targetManualRepo.GetAll().ToList();

                var manualCosts = new List<HardwareManualCost>();

                Console.WriteLine("Creating portfolio...");
                CreatePortfolio(sourcePortfolio, targetPortfolioRepo, context.PrincipalPortfolio);
                Console.WriteLine("Portfolio created");
                Console.WriteLine("Manual costs coppying...");

                foreach (var mc in sourceManualCosts)
                {
                    var portfolio = GetPortfolioByDependencies(mc.LocalPortfolio, targetPortfolioRepo, true);

                    var manualCost = targetManualCosts.FirstOrDefault(tc => tc.Id == portfolio.Id) ?? new HardwareManualCost {LocalPortfolio = portfolio};


                    //manualCost.DealerDiscount = mc.DealerDiscount;
                    //manualCost.ListPrice = mc.ListPrice;
                    //manualCost.ServiceTC = mc.ServiceTC;
                    manualCost.ServiceTP = mc.ServiceTP;
                    //manualCost.ReleaseDate = mc.ReleaseDate;
                    manualCost.ServiceTP1_Released = mc.ServiceTP1_Released;
                    manualCost.ServiceTP2_Released = mc.ServiceTP2_Released;
                    manualCost.ServiceTP3_Released = mc.ServiceTP3_Released;
                    manualCost.ServiceTP4_Released = mc.ServiceTP4_Released;
                    manualCost.ServiceTP5_Released = mc.ServiceTP5_Released;
                    manualCost.ChangeUser = editor;
                    

                   manualCosts.Add(manualCost);
                }

                targetManualRepo.Save(manualCosts);
                targetRepoSet.Sync();
            }

        }

        private void LoadDependencies()
        {
            var wgRepo = kernel.Get<IRepository<TE.Wg>>();
            Dependencies[nameof(TE.Wg)] = wgRepo.GetAll().ToDictionary(wg => wg.Name, wg => wg.Id);

            var countryRepo = kernel.Get<IRepository<TE.Country>>();
            Dependencies[nameof(TE.Country)] = countryRepo.GetAll().ToDictionary(c => c.Name, c => c.Id);

            var availabilityRepo = kernel.Get<IRepository<TE.Availability>>();
            Dependencies[nameof(TE.Availability)] = availabilityRepo.GetAll().ToDictionary(av => av.Name, av => av.Id);

            var durationRepo = kernel.Get<IRepository<TE.Duration>>();
            Dependencies[nameof(TE.Duration)] = durationRepo.GetAll().ToDictionary(d => d.Name, d => d.Id);

            var proActiveRepo = kernel.Get<IRepository<TE.ProActiveSla>>();
            Dependencies[nameof(TE.ProActiveSla)] = proActiveRepo.GetAll().ToDictionary(pa => pa.Name, pa => pa.Id);

            var reactionTimeRepo = kernel.Get<IRepository<TE.ReactionTime>>();
            Dependencies[nameof(TE.ReactionTime)] = reactionTimeRepo.GetAll().ToDictionary(rt => rt.Name, rt => rt.Id);

            var reactionTypeRepo = kernel.Get<IRepository<TE.ReactionType>>();
            Dependencies[nameof(TE.ReactionType)] = reactionTypeRepo.GetAll().ToDictionary(rt => rt.Name, rt => rt.Id);

            var serviceLocationRepo = kernel.Get<IRepository<TE.ServiceLocation>>();
            Dependencies[nameof(TE.ServiceLocation)] = serviceLocationRepo
                .GetAll().ToDictionary(sl => sl.Name, sl => sl.Id);
        }



        private void CreatePortfolio(IEnumerable<LocalPortfolio> sourcePortfolio, 
            IRepository<TargetPortfolio.LocalPortfolio> targetRepo, 
            IQueryable<PrincipalPortfolio> sourcePrincipalPortfolios)
        {
            var portfolioService = kernel.Get<PortfolioService>();

            foreach (var portfolio in sourcePortfolio)
            {
                var targetPortfolio = GetPortfolioByDependencies(portfolio, targetRepo, false);

                if (targetPortfolio == null)
                {
                    //first check Master Portfolio
                    var sourcePrincipalPortfolio = sourcePrincipalPortfolios.First(p => p.WgId == portfolio.WgId
                                                                                        && p.AvailabilityId ==
                                                                                        portfolio.AvailabilityId &&
                                                                                        p.ProActiveSlaId ==
                                                                                        portfolio.ProActiveSlaId &&
                                                                                        p.ReactionTimeId ==
                                                                                        portfolio.ReactionTimeId
                                                                                        && p.ReactionTypeId ==
                                                                                        portfolio.ReactionTypeId &&
                                                                                        p.ServiceLocationId ==
                                                                                        portfolio.ServiceLocationId &&
                                                                                        p.DurationId ==
                                                                                        portfolio.DurationId);

                    PortfolioRuleSetDto portfolioRule = new PortfolioRuleSetDto
                    {
                        Availabilities = new[] {Dependencies[nameof(TE.Availability)][portfolio.Availability.Name]},
                        Durations = new[] {Dependencies[nameof(TE.Duration)][portfolio.Duration.Name]},
                        ProActives = new[] {Dependencies[nameof(TE.ProActiveSla)][portfolio.ProActiveSla.Name]},
                        ReactionTimes = new[] {Dependencies[nameof(TE.ReactionTime)][portfolio.ReactionTime.Name]},
                        ReactionTypes = new[] {Dependencies[nameof(TE.ReactionType)][portfolio.ReactionType.Name]},
                        ServiceLocations = new[]
                            {Dependencies[nameof(TE.ServiceLocation)][portfolio.ServiceLocation.Name]},
                        Wgs = new[] {Dependencies[nameof(TE.Wg)][portfolio.Wg.Name]},
                        IsCorePortfolio = sourcePrincipalPortfolio.IsCorePortfolio,
                        IsGlobalPortfolio = sourcePrincipalPortfolio.IsGlobalPortfolio,
                        IsMasterPortfolio = sourcePrincipalPortfolio.IsMasterPortfolio
                    };



                    portfolioService.Allow(portfolioRule);

                    portfolioRule.CountryId = Dependencies[nameof(TE.Country)][portfolio.Country.Name];
                    portfolioService.Allow(portfolioRule);
                }
            }
        }


        private TargetPortfolio.LocalPortfolio GetPortfolioByDependencies(LocalPortfolio sourcePortfolio, 
            IRepository<TargetPortfolio.LocalPortfolio> targetRepo, bool throwEx)
        {
            var portfolio = targetRepo.GetAll().FirstOrDefault(p =>
                p.Country.Name == sourcePortfolio.Country.Name &&
                p.Availability.Name == sourcePortfolio.Availability.Name &&
                p.Duration.Name == sourcePortfolio.Duration.Name &&
                p.ProActiveSla.Name == sourcePortfolio.ProActiveSla.Name &&
                p.ReactionTime.Name == sourcePortfolio.ReactionTime.Name &&
                p.ReactionType.Name == sourcePortfolio.ReactionType.Name &&
                p.ServiceLocation.Name == sourcePortfolio.ServiceLocation.Name &&
                p.Wg.Name == sourcePortfolio.Wg.Name);

            if (portfolio == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                var message = $"Missing portfolio for Country: {sourcePortfolio.Country.Name}," +
                              $"Availability: {sourcePortfolio.Availability.Name}," +
                              $"Duration: {sourcePortfolio.Duration.Name}, ProActiveSla: {sourcePortfolio.ProActiveSla.Name}, +" +
                              $"ReactionTime: {sourcePortfolio.ReactionTime.Name}, ReactionType: {sourcePortfolio.ReactionType.Name}, +" +
                              $"ServiceLocation: {sourcePortfolio.ServiceLocation.Name}";
                Console.WriteLine(message);

                if (throwEx)
                    throw new Exception(message);
            }

            return portfolio;
        }
    }
}
