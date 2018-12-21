using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.BusinessLogicLayer.Procedures;
using Gdc.Scd.Core.Entities.Calculation;
using Gdc.Scd.Core.Entities.CapabilityMatrix;
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

        private readonly IRepository<CapabilityMatrix> matrixRepo;

        private readonly IRepository<HardwareManualCost> hwManualRepo;

        private readonly IRepository<SoftwareMaintenance> swMaintenanceRepo;

        private readonly IRepository<SoftwareProactive> swProactiveRepo;


        public CalculationService(
                IRepositorySet repositorySet,
                IRepository<HardwareManualCost> hwManualRepo,
                IRepository<SoftwareMaintenance> swMaintenanceRepo,
                IRepository<SoftwareProactive> swProactiveRepo,
                IRepository<CapabilityMatrix> matrixRepo,
                IUserService userService
            )
        {
            this.repositorySet = repositorySet;
            this.hwManualRepo = hwManualRepo;
            this.swMaintenanceRepo = swMaintenanceRepo;
            this.swProactiveRepo = swProactiveRepo;
            this.matrixRepo = matrixRepo;     
        }

        public async Task<JsonArrayDto> GetHardwareCost(bool approved, HwFilterDto filter, int lasId, int limit)
        {
            var query = matrixRepo.GetAll().Where(x => !x.Denied);

            if (filter != null)
            {
                query = query.WhereIf(filter.Country.HasValue, x => x.Country.Id == filter.Country.Value)
                             .WhereIf(filter.Wg.HasValue, x => x.Wg.Id == filter.Wg.Value)
                             .WhereIf(filter.Availability.HasValue, x => x.Availability.Id == filter.Availability.Value)
                             .WhereIf(filter.Duration.HasValue, x => x.Duration.Id == filter.Duration.Value)
                             .WhereIf(filter.ReactionType.HasValue, x => x.ReactionType.Id == filter.ReactionType.Value)
                             .WhereIf(filter.ReactionTime.HasValue, x => x.ReactionTime.Id == filter.ReactionTime.Value)
                             .WhereIf(filter.ServiceLocation.HasValue, x => x.ServiceLocation.Id == filter.ServiceLocation.Value);
            }

            var res = await new GetHwCost(repositorySet).ExecuteJsonAsync(approved, filter, lasId, limit);
            
            res.Total = await query.Select(x => x.Id).GetCountAsync();

            return res;
        }

        public async Task<Tuple<SwMaintenanceCostDto[], int>> GetSoftwareCost(SwFilterDto filter, int start, int limit)
        {
            var query = swMaintenanceRepo.GetAll();

            if (filter != null)
            {
                query = query.WhereIf(filter.Sog.HasValue, x => x.Sog == filter.Sog.Value)
                             .WhereIf(filter.Availability.HasValue, x => x.Availability == filter.Availability.Value)
                             .WhereIf(filter.Year.HasValue, x => x.Year == filter.Year.Value);
            }

            var count = await query.GetCountAsync();

            query = query.WithPaging(start, limit);

            var result = await query.Select(x => new SwMaintenanceCostDto
            {
                Sog = x.SogRef.Name,
                Availability = x.AvailabilityRef.Name,
                Year = x.YearRef.Name,

                DealerPrice = x.DealerPrice,
                DealerPrice_Approved = x.DealerPrice_Approved,

                MaintenanceListPrice = x.MaintenanceListPrice,
                MaintenanceListPrice_Approved = x.MaintenanceListPrice_Approved,

                Reinsurance = x.Reinsurance,
                Reinsurance_Approved = x.Reinsurance_Approved,

                ServiceSupport = x.ServiceSupport,
                ServiceSupport_Approved = x.ServiceSupport_Approved,

                TransferPrice = x.TransferPrice,
                TransferPrice_Approved = x.TransferPrice_Approved
            }).GetAsync();

            return new Tuple<SwMaintenanceCostDto[], int>(result, count);
        }

        public async Task<Tuple<SwProactiveCostDto[], int>> GetSoftwareProactiveCost(SwFilterDto filter, int start, int limit)
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

            var result = await query.Select(x => new SwProactiveCostDto
            {
                Country = x.CountryRef.Name,
                Sog = x.SogRef.Name,
                Year = x.YearRef.Name,

                ProActive = x.ProActive,
                ProActive_Approved = x.ProActive_Approved
            }).GetAsync();

            return new Tuple<SwProactiveCostDto[], int>(result, count);
        }

        public void SaveHardwareCost(IEnumerable<HwCostManualDto> records)
        {
            var recordsId = records.Select(x => x.Id);

            var entities = (from m in matrixRepo.GetAll().Where(x => recordsId.Contains(x.Id))
                            from hw in hwManualRepo.GetAll().Where(x => x.Id == m.Id).DefaultIfEmpty()
                            select new
                            {
                                Matrix = m,
                                m.Country,
                                Manual = hw
                            })
                           .ToDictionary(x => x.Matrix.Id, y => y);

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
                    var m = e.Matrix;
                    var hwManual = e.Manual ?? new HardwareManualCost { Matrix = m }; //create new if does not exist

                    if (country.CanOverrideTransferCostAndPrice)
                    {
                        hwManual.ServiceTC = rec.ServiceTC;
                        hwManual.ServiceTP = rec.ServiceTP;
                        //
                        hwManualRepo.Save(hwManual);
                    }

                    if (country.CanStoreListAndDealerPrices)
                    {
                        hwManual.ListPrice = rec.ListPrice;
                        hwManual.DealerDiscount = rec.DealerDiscount;
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
