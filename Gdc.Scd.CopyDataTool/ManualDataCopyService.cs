using Gdc.Scd.BusinessLogicLayer.Dto.Portfolio;
using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.CopyDataTool.Configuration;
using Gdc.Scd.CopyDataTool.Entities;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.Calculation;
using Gdc.Scd.Core.Entities.Portfolio;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.CopyDataTool
{
    public class ManualDataCopyService
    {
        private readonly IKernel kernel;
        private readonly CopyDetailsConfig config;
        private readonly Dictionary<string, Dictionary<string, long>> Dependencies;
        private readonly EntityFrameworkRepositorySet sourceRepositorySet;
        private readonly ExcangeRateCalculator excangeRateCalculator;

        public ManualDataCopyService(IKernel kernel, ExcangeRateCalculator excangeRateCalculator)
        {
            this.kernel = kernel;
            this.sourceRepositorySet = new EntityFrameworkRepositorySet(this.kernel, "SourceDB");
            this.excangeRateCalculator = excangeRateCalculator;
            config = this.kernel.Get<CopyDetailsConfig>();
            Dependencies = new Dictionary<string, Dictionary<string, long>>();
            LoadDependencies();
        }

        public void CopyData()
        {
            if (config.CopyManualCosts)
            {
                Console.WriteLine("Manual costs coppying...");

                var userRepository = kernel.Get<IRepository<User>>();
                var changedUser = userRepository.GetAll().FirstOrDefault(user => user.Login == config.EditUser);

                if (changedUser == null)
                    throw new Exception($"User {config.EditUser} could not be found in the target database");

                UpdateStandardWarrantyManual(changedUser);
                UpdateHardwareManualCosts(changedUser);

                Console.WriteLine("Сopying completed");
            }
        }

        private void UpdateStandardWarrantyManual(User editor)
        {
            var sourceStandardWarrs =
                this.sourceRepositorySet.GetRepository<StandardWarrantyManualCost>()
                                        .GetAll();

            if (this.config.HasCountry)
            {
                sourceStandardWarrs = sourceStandardWarrs.Where(sw => sw.Country.Name == config.Country);
            }

            if (this.config.HasExcludedWgs)
            {
                var excludedWgs = config.GetExcludedWgs();
                sourceStandardWarrs = sourceStandardWarrs.Where(mc => !excludedWgs.Contains(mc.Wg.Name));
            }

            var targetWarrantyRepoSet = kernel.Get<IRepositorySet>();
            var targetWarrantyRepo = targetWarrantyRepoSet.GetRepository<StandardWarrantyManualCost>();
            var targetWarrantiesToUpdate = new List<StandardWarrantyManualCost>();

            foreach (var sourceStandardWarr in sourceStandardWarrs)
            {
                var country = this.config.GetTargetCountry(sourceStandardWarr.Country.Name);
                var targetWarrToUpdate = 
                    targetWarrantyRepo.GetAll()
                                      .FirstOrDefault(sw => sw.Wg.Name == sourceStandardWarr.Wg.Name && sw.Country.Name == country) 
                                      ?? new StandardWarrantyManualCost
                                      {
                                          WgId = Dependencies[nameof(Wg)][sourceStandardWarr.Wg.Name],
                                          CountryId = Dependencies[nameof(Country)][country]
                                      };

                targetWarrToUpdate.ChangeDate = DateTime.Now;
                targetWarrToUpdate.ChangeUser = editor;
                targetWarrToUpdate.StandardWarranty = sourceStandardWarr.StandardWarranty;

                targetWarrantiesToUpdate.Add(targetWarrToUpdate);
            }

            targetWarrantyRepo.Save(targetWarrantiesToUpdate);
            targetWarrantyRepoSet.Sync();
        }

        private void UpdateHardwareManualCosts(User editor)
        {
            var sourceManualCosts =
                this.sourceRepositorySet.GetRepository<HardwareManualCost>()
                                        .GetAll()
                                        .Include(mc => mc.LocalPortfolio);

            if (this.config.HasCountry)
            {
                sourceManualCosts = sourceManualCosts.Where(mc => mc.LocalPortfolio.Country.Name == config.Country);
            }

            if (this.config.HasExcludedWgs)
            {
                var excludedWgs = config.GetExcludedWgs();
                sourceManualCosts = sourceManualCosts.Where(mc => !excludedWgs.Contains(mc.LocalPortfolio.Wg.Name));
            }

            var sourcePortfolio = sourceManualCosts.Select(c => c.LocalPortfolio).ToList();
            var targetRepoSet = kernel.Get<IRepositorySet>();
            var targetManualRepo = targetRepoSet.GetRepository<HardwareManualCost>();
            var targetPortfolioRepo = targetRepoSet.GetRepository<LocalPortfolio>();

            var targetManualCosts = targetManualRepo.GetAll().ToList();

            var manualCosts = new List<HardwareManualCost>();

            Console.WriteLine("Creating portfolio...");
            this.CreatePortfolio(sourcePortfolio, targetPortfolioRepo, this.sourceRepositorySet.GetRepository<PrincipalPortfolio>().GetAll());
            Console.WriteLine("Portfolio created");
            Console.WriteLine("Manual costs coppying...");

            foreach (var mc in sourceManualCosts)
            {
                var portfolio = GetPortfolioByDependencies(mc.LocalPortfolio, targetPortfolioRepo, true);
                var manualCost = targetManualCosts.FirstOrDefault(tc => tc.Id == portfolio.Id) ?? new HardwareManualCost {LocalPortfolio = portfolio};

                manualCost.DealerDiscount = mc.DealerDiscount;
                manualCost.ListPrice = this.excangeRateCalculator.Calculate(mc.ListPrice);
                manualCost.ServiceTC = this.excangeRateCalculator.Calculate(mc.ServiceTC);
                manualCost.ServiceTP = this.excangeRateCalculator.Calculate(mc.ServiceTP);
                manualCost.ReleaseDate = mc.ReleaseDate;
                manualCost.ServiceTP1_Released = this.excangeRateCalculator.Calculate(mc.ServiceTP1_Released);
                manualCost.ServiceTP2_Released = this.excangeRateCalculator.Calculate(mc.ServiceTP2_Released);
                manualCost.ServiceTP3_Released = this.excangeRateCalculator.Calculate(mc.ServiceTP3_Released);
                manualCost.ServiceTP4_Released = this.excangeRateCalculator.Calculate(mc.ServiceTP4_Released);
                manualCost.ServiceTP5_Released = this.excangeRateCalculator.Calculate(mc.ServiceTP5_Released);
                manualCost.ServiceTP_Released = this.excangeRateCalculator.Calculate(mc.ServiceTP_Released);
                manualCost.ChangeUser = editor;
                manualCost.ChangeUserId = editor.Id;
                manualCost.ReleaseUser = mc.ReleaseUser;
                manualCost.ReleaseUserId = mc.ReleaseUserId;

                manualCosts.Add(manualCost);
            }

            targetManualRepo.Save(manualCosts);
            targetRepoSet.Sync();
        }

        private void LoadDependencies()
        {
            var wgRepo = kernel.Get<IRepository<Wg>>();
            Dependencies[nameof(Wg)] = wgRepo.GetAll().ToDictionary(wg => wg.Name, wg => wg.Id);

            var countryRepo = kernel.Get<IRepository<Country>>();
            Dependencies[nameof(Country)] = countryRepo.GetAll().ToDictionary(c => c.Name, c => c.Id);

            var availabilityRepo = kernel.Get<IRepository<Availability>>();
            Dependencies[nameof(Availability)] = availabilityRepo.GetAll().ToDictionary(av => av.Name, av => av.Id);

            var durationRepo = kernel.Get<IRepository<Duration>>();
            Dependencies[nameof(Duration)] = durationRepo.GetAll().ToDictionary(d => d.Name, d => d.Id);

            var proActiveRepo = kernel.Get<IRepository<ProActiveSla>>();
            Dependencies[nameof(ProActiveSla)] = proActiveRepo.GetAll().ToDictionary(pa => pa.Name, pa => pa.Id);

            var reactionTimeRepo = kernel.Get<IRepository<ReactionTime>>();
            Dependencies[nameof(ReactionTime)] = reactionTimeRepo.GetAll().ToDictionary(rt => rt.Name, rt => rt.Id);

            var reactionTypeRepo = kernel.Get<IRepository<ReactionType>>();
            Dependencies[nameof(ReactionType)] = reactionTypeRepo.GetAll().ToDictionary(rt => rt.Name, rt => rt.Id);

            var serviceLocationRepo = kernel.Get<IRepository<ServiceLocation>>();
            Dependencies[nameof(ServiceLocation)] = serviceLocationRepo
                .GetAll().ToDictionary(sl => sl.Name, sl => sl.Id);
        }

        private void CreatePortfolio(
            IEnumerable<LocalPortfolio> sourcePortfolio, 
            IRepository<LocalPortfolio> targetRepo, 
            IQueryable<PrincipalPortfolio> sourcePrincipalPortfolios)
        {
            var portfolioService = kernel.Get<PortfolioService>();

            foreach (var portfolio in sourcePortfolio)
            {
                var targetPortfolio = GetPortfolioByDependencies(portfolio, targetRepo, false);
                if (targetPortfolio == null)
                {
                    //first check Master Portfolio
                    var sourcePrincipalPortfolio = 
                        sourcePrincipalPortfolios.First(
                            p => 
                                p.Wg.Id == portfolio.Wg.Id && 
                                p.Availability.Id == portfolio.Availability.Id &&
                                p.ProActiveSla.Id == portfolio.ProActiveSla.Id &&
                                p.ReactionTime.Id == portfolio.ReactionTime.Id && 
                                p.ReactionType.Id == portfolio.ReactionType.Id &&
                                p.ServiceLocation.Id == portfolio.ServiceLocation.Id &&
                                p.Duration.Id == portfolio.Duration.Id);

                    var portfolioRule = new PortfolioRuleSetDto
                    {
                        Availabilities = new[] {Dependencies[nameof(Availability)][portfolio.Availability.Name]},
                        Durations = new[] {Dependencies[nameof(Duration)][portfolio.Duration.Name]},
                        ProActives = new[] {Dependencies[nameof(ProActiveSla)][portfolio.ProActiveSla.Name]},
                        ReactionTimes = new[] {Dependencies[nameof(ReactionTime)][portfolio.ReactionTime.Name]},
                        ReactionTypes = new[] {Dependencies[nameof(ReactionType)][portfolio.ReactionType.Name]},
                        ServiceLocations = new[] {Dependencies[nameof(ServiceLocation)][portfolio.ServiceLocation.Name]},
                        Wgs = new[] {Dependencies[nameof(Wg)][portfolio.Wg.Name]},
                        IsCorePortfolio = sourcePrincipalPortfolio.IsCorePortfolio,
                        IsGlobalPortfolio = sourcePrincipalPortfolio.IsGlobalPortfolio,
                        IsMasterPortfolio = sourcePrincipalPortfolio.IsMasterPortfolio
                    };

                    portfolioService.Allow(portfolioRule);

                    var country = this.config.GetTargetCountry(portfolio.Country.Name);

                    portfolioRule.CountryId = Dependencies[nameof(Country)][country];
                    portfolioService.Allow(portfolioRule);
                }
            }
        }

        private LocalPortfolio GetPortfolioByDependencies(
            LocalPortfolio sourcePortfolio, 
            IRepository<LocalPortfolio> targetRepo, 
            bool throwEx)
        {
            var countryName = this.config.GetTargetCountry(sourcePortfolio.Country.Name);
            var portfolio = targetRepo.GetAll().FirstOrDefault(p =>
                p.Country.Name == countryName &&
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
                var message = $"Missing portfolio for Country: {countryName}," +
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
