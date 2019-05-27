using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.BusinessLogicLayer.Procedures;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.Calculation;
using Gdc.Scd.Core.Entities.Portfolio;
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

        private readonly IRepository<StandardWarrantyManualCost> stdwRepo;

        public CalculationService(
                IRepositorySet repositorySet,
                IRepository<HardwareManualCost> hwManualRepo,
                IRepository<LocalPortfolio> portfolioRepo,
                IRepository<StandardWarrantyManualCost> stdwRepo
            )
        {
            this.repositorySet = repositorySet;
            this.hwManualRepo = hwManualRepo;
            this.portfolioRepo = portfolioRepo;
            this.stdwRepo = stdwRepo;
        }

        public Task<(string json, int total)> GetHardwareCost(bool approved, HwFilterDto filter, int lastId, int limit)
        {
            if (filter == null || filter.Country == null || filter.Country.Length == 0)
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

        public Task<(string json, int total)> GetSoftwareProactiveCost(
            bool approved,
            SwFilterDto filter,
            int lastId,
            int limit
        )
        {
            return new GetSwProActiveCost(repositorySet).ExecuteJsonAsync(approved, filter, lastId, limit);
        }

        public async Task ReleaseHardwareCost(User changeUser, HwFilterDto filter)
        {
            if (filter?.Country == null || filter.Country.Length == 0)
            {
                throw new ArgumentException("No country specified");
            }

            await new ReleaseHwCost(repositorySet).ExecuteAsync(changeUser.Id, filter);
        }

        public async Task ReleaseSelectedHardwareCost(User changeUser, HwFilterDto filter, HwCostDto[] items)
        {
            if (items == null || items.Length == 0)
            {
                throw new ArgumentException("No records specified");
            }

            await new ReleaseHwCost(repositorySet).ExecuteAsync(changeUser.Id, filter, items);
        }

        public void SaveHardwareCost(User changeUser, IEnumerable<HwCostManualDto> records)
        {
            var recordsId = records.Select(x => x.Id);

            var entities = (from p in portfolioRepo.GetAll().Where(x => recordsId.Contains(x.Id))
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
                    var hwManual = e.Manual ?? new HardwareManualCost
                    { LocalPortfolio = p }; //create new if does not exist

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
                transaction?.Rollback();
                throw;
            }
            finally
            {
                transaction?.Dispose();
            }
        }

        public void SaveStandardWarrantyCost(User changeUser, HwCostDto[] records)
        {
            new UpdateStandardWarrantyManualCost(repositorySet).Execute(changeUser.Id, records);
        }
    }
}