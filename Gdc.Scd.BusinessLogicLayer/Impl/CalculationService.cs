using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities.Calculation;
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

        private readonly IRepository<HardwareCalculationResult> hwRepo;

        private readonly IRepository<SoftwareCalculationResult> swRepo;

        public CalculationService(
                IRepositorySet repositorySet,
                IRepository<HardwareCalculationResult> hwRepo,
                IRepository<SoftwareCalculationResult> swRepo
            )
        {
            this.repositorySet = repositorySet;
            this.hwRepo = hwRepo;
            this.swRepo = swRepo;
        }

        public Task<Tuple<HwCostDto[], int>> GetHardwareCost(HwFilterDto filter, int start, int limit)
        {
            var query = hwRepo.GetAll();

            if (filter != null)
            {
                query = query.WhereIf(filter.Country.HasValue, x => x.CountryId == filter.Country.Value)
                             .WhereIf(filter.Wg.HasValue, x => x.WgId == filter.Wg.Value)
                             .WhereIf(filter.Availability.HasValue, x => x.AvailabilityId == filter.Availability.Value)
                             .WhereIf(filter.Duration.HasValue, x => x.DurationId == filter.Duration.Value)
                             .WhereIf(filter.ReactionType.HasValue, x => x.ReactionTypeId == filter.ReactionType.Value)
                             .WhereIf(filter.ReactionTime.HasValue, x => x.ReactionTimeId == filter.ReactionTime.Value)
                             .WhereIf(filter.ServiceLocation.HasValue, x => x.ServiceLocationId == filter.ServiceLocation.Value);
            }

            var result = query.Select(hw => new HwCostDto
            {
                Id = hw.Id,

                Country = hw.Country,
                Wg = hw.Wg,
                Availability = hw.Availability,
                Duration = hw.Duration,
                ReactionTime = hw.ReactionTime,
                ReactionType = hw.ReactionType,
                ServiceLocation = hw.ServiceLocation,

                AvailabilityFee = hw.AvailabilityFee,
                AvailabilityFee_Approved = hw.AvailabilityFee_Approved,

                Credits = hw.Credits,
                Credits_Approved = hw.Credits_Approved,

                FieldServiceCost = hw.FieldServiceCost,
                FieldServiceCost_Approved = hw.FieldServiceCost_Approved,

                HddRetention = hw.HddRetention,
                HddRetention_Approved = hw.HddRetention_Approved,

                LocalServiceStandardWarranty = hw.LocalServiceStandardWarranty,
                LocalServiceStandardWarranty_Approved = hw.LocalServiceStandardWarranty_Approved,

                Logistic = hw.Logistic,
                Logistic_Approved = hw.Logistic_Approved,

                MaterialW = hw.MaterialW,
                MaterialW_Approved = hw.MaterialW_Approved,

                MaterialOow = hw.MaterialOow,
                MaterialOow_Approved = hw.MaterialOow_Approved,

                TaxAndDutiesW = hw.TaxAndDutiesW,
                TaxAndDutiesW_Approved = hw.TaxAndDutiesW_Approved,

                TaxAndDutiesOow = hw.TaxAndDutiesOow,
                TaxAndDutiesOow_Approved = hw.TaxAndDutiesOow_Approved,

                OtherDirect = hw.OtherDirect,
                OtherDirect_Approved = hw.OtherDirect_Approved,

                ProActive = hw.ProActive,
                ProActive_Approved = hw.ProActive_Approved,

                Reinsurance = hw.Reinsurance,
                Reinsurance_Approved = hw.Reinsurance,

                ServiceSupport = hw.ServiceSupport,
                ServiceSupport_Approved = hw.ServiceSupport_Approved,

                ServiceTC = hw.ServiceTC,
                ServiceTC_Approved = hw.ServiceTC_Approved,

                ServiceTCManual = hw.ServiceTCManual,
                ServiceTCManual_Approved = hw.ServiceTCManual_Approved,

                ServiceTP = hw.ServiceTP,
                ServiceTP_Approved = hw.ServiceTP_Approved,

                ServiceTPManual = hw.ServiceTPManual,
                ServiceTPManual_Approved = hw.ServiceTPManual_Approved
            });

            return result.PagingWithCountAsync(start, limit);
        }

        public Task<Tuple<SwCostDto[], int>> GetSoftwareCost(SwFilterDto filter, int start, int limit)
        {
            var query = swRepo.GetAll();

            if (filter != null)
            {
                query = query.WhereIf(filter.Country.HasValue, x => x.CountryId == filter.Country.Value)
                             .WhereIf(filter.Sog.HasValue, x => x.SogId == filter.Sog.Value)
                             .WhereIf(filter.Availability.HasValue, x => x.AvailabilityId == filter.Availability.Value)
                             .WhereIf(filter.Year.HasValue, x => x.YearId == filter.Year.Value);
            }

            query = query.OrderBy(x => x.Id);

            var result = query.Select(x => new SwCostDto
            {
                Country = x.Country,
                Sog = x.Sog,
                Availability = x.Availability,
                Year = x.Year,

                DealerPrice = x.DealerPrice,
                DealerPrice_Approved = x.DealerPrice_Approved,

                MaintenanceListPrice = x.MaintenanceListPrice,
                MaintenanceListPrice_Approved = x.MaintenanceListPrice_Approved,

                Reinsurance = x.Reinsurance,
                Reinsurance_Approved = x.Reinsurance_Approved,

                ServiceSupport = x.ServiceSupport,
                ServiceSupport_Approved = x.ServiceSupport_Approved,

                TransferPrice = x.TransferPrice,
                TransferPrice_Approved = x.TransferPrice_Approved,

                ProActive = x.ProActive,
                ProActive_Approved = x.ProActive_Approved
            });

            return result.PagingWithCountAsync(start, limit);
        }

        public void SaveHardwareCost(IEnumerable<HwCostManualDto> records)
        {
            var recordsId = records.Select(x => x.Id);

            var query = hwRepo.GetAll()
                              .Where(x => recordsId.Contains(x.Id) &&
                                          x.CountryRef.CanOverrideTransferCostAndPrice);

            var entities = query.ToDictionary(x => x.Id, y => y);

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
                    if (entities.ContainsKey(rec.Id))
                    {
                        var e = entities[rec.Id];

                        e.ServiceTCManual = rec.ServiceTC;
                        e.ServiceTPManual = rec.ServiceTP;

                        hwRepo.Save(e);
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
