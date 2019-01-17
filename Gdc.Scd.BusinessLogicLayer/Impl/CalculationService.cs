using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.BusinessLogicLayer.Procedures;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.Calculation;
using Gdc.Scd.Core.Entities.Portfolio;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CalculationService : ICalculationService
    {
        private readonly IRepositorySet repositorySet;

        private readonly IRepository<LocalPortfolio> portfolioRepo;

        private readonly IRepository<HardwareManualCost> hwManualRepo;

        private readonly IRepository<SoftwareMaintenance> swMaintenanceRepo;

        private readonly IRepository<SoftwareProactive> swProactiveRepo;

        public CalculationService(
                IRepositorySet repositorySet,
                IRepository<HardwareManualCost> hwManualRepo,
                IRepository<SoftwareMaintenance> swMaintenanceRepo,
                IRepository<SoftwareProactive> swProactiveRepo,
                IRepository<LocalPortfolio> portfolioRepo
            )
        {
            this.repositorySet = repositorySet;
            this.hwManualRepo = hwManualRepo;
            this.swMaintenanceRepo = swMaintenanceRepo;
            this.swProactiveRepo = swProactiveRepo;
            this.portfolioRepo = portfolioRepo;
        }

        public Task<(string json, int total)> GetHardwareCost(bool approved, HwFilterDto filter, int lastId, int limit)
        {
            if (filter == null || filter.Country <= 0)
            {
                throw new ArgumentException("No country specified");
            }

            return new GetHwCost(repositorySet).ExecuteJsonAsync(approved, filter, lastId, limit);
        }

        public Task<(string json, int total)> GetSoftwareCost(
                bool approved,
                SwFilterDto filter,
                int lastId,
                int limit
            )
        {
            return new GetSwCost(repositorySet).ExecuteJsonAsync(approved, filter, lastId, limit);
        }

        public async Task<(SwProactiveCostDto[] items, int total)> GetSoftwareProactiveCost(
                bool approved,
                SwFilterDto filter,
                int start,
                int limit
            )
        {
            var query = swProactiveRepo.GetAll();

            if (filter != null)
            {
                query = query.WhereIf(filter.Country.HasValue, x => x.Country == filter.Country.Value)
                             .WhereIf(filter.Sog.HasValue, x => x.Sog == filter.Sog.Value)
                             .WhereIf(filter.Year.HasValue, x => x.Year == filter.Year.Value);
            }

            var count = await query.GetCountAsync();

            query = query.WithPaging(start, limit);

            IQueryable<SwProactiveCostDto> selectQuery;

            if (approved)
            {
                selectQuery = query.Select(x => new SwProactiveCostDto
                {
                    Country = x.CountryRef.Name,
                    Sog = x.SogRef.Name,
                    Year = x.YearRef.Name,
                    ProActive = x.ProActive_Approved
                });
            }
            else
            {
                selectQuery = query.Select(x => new SwProactiveCostDto
                {
                    Country = x.CountryRef.Name,
                    Sog = x.SogRef.Name,
                    Year = x.YearRef.Name,
                    ProActive = x.ProActive
                });
            }

            var result = await selectQuery.GetAsync();

            return (result, count);
        }

        public void SaveHardwareCost(User changeUser, long countryId, IEnumerable<HwCostManualDto> records)
        {
            var recordsId = records.Select(x => x.Id);

            var entities = (from p in portfolioRepo.GetAll().Where(x => x.Country.Id == countryId && recordsId.Contains(x.Id))
                            from hw in hwManualRepo.GetAll().Where(x => x.Id == p.Id).DefaultIfEmpty()
                            select new
                            {
                                Portfolio = p,
                                p.Country,
                                Manual = hw
                            })
                           .ToDictionary(x => x.Portfolio.Id, y => y);

            if (entities.Count == 0)
            {
                return;
            }

            ITransaction transaction = null;
            try
            {
                transaction = repositorySet.GetTransaction();

                foreach (var rec in records)
                {
                    if (!entities.ContainsKey(rec.Id))
                    {
                        continue;
                    }

                    var e = entities[rec.Id];
                    var country = e.Country;
                    var p = e.Portfolio;
                    var hwManual = e.Manual ?? new HardwareManualCost { LocalPortfolio = p }; //create new if does not exist

                    if (country.CanOverrideTransferCostAndPrice)
                    {
                        hwManual.ServiceTC = rec.ServiceTC;
                        hwManual.ServiceTP = rec.ServiceTP;
                        hwManual.ChangeUser = changeUser;
                        //
                        hwManualRepo.Save(hwManual);
                    }

                    if (country.CanStoreListAndDealerPrices)
                    {
                        hwManual.ListPrice = rec.ListPrice;
                        hwManual.DealerDiscount = rec.DealerDiscount;
                        hwManual.ChangeUser = changeUser;
                        //
                        hwManualRepo.Save(hwManual);
                    }
                }

                repositorySet.Sync();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Dispose();
                }
            }
        }
    }
}
