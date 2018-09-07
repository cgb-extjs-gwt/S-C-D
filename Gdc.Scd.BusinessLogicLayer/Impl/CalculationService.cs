using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities.Calculation;
using Gdc.Scd.Core.Entities.CapabilityMatrix;
using Gdc.Scd.Core.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CalculationService : ICalculationService
    {
        private readonly IRepositorySet repositorySet;

        private readonly IRepository<HardwareCalculationResult> hwRepo;

        private readonly IRepository<CapabilityMatrixCountryAllowView> matrixRepo;

        private readonly IRepository<SoftwareCalculationResult> swRepo;

        public CalculationService(
                IRepositorySet repositorySet,
                IRepository<HardwareCalculationResult> hwRepo,
                IRepository<SoftwareCalculationResult> swRepo,
                IRepository<CapabilityMatrixCountryAllowView> matrixRepo
            )
        {
            this.repositorySet = repositorySet;
            this.hwRepo = hwRepo;
            this.swRepo = swRepo;
            this.matrixRepo = matrixRepo;
        }

        public IEnumerable<HwCostDto> GetHardwareCost(HwFilterDto filter, int start, int limit, out int count)
        {
            var query = from m in matrixRepo.GetAll()
                        from hw in hwRepo.GetAll()
                        where m.Id == hw.Id
                        select new { m, hw };

            if (filter != null)
            {
                query = query.WhereIf(filter.Country.HasValue, x => x.m.CountryId == filter.Country.Value)
                             .WhereIf(filter.Wg.HasValue, x => x.m.WgId == filter.Wg.Value)
                             .WhereIf(filter.Availability.HasValue, x => x.m.AvailabilityId == filter.Availability.Value)
                             .WhereIf(filter.Duration.HasValue, x => x.m.DurationId == filter.Duration.Value)
                             .WhereIf(filter.ReactionType.HasValue, x => x.m.ReactionTypeId == filter.ReactionType.Value)
                             .WhereIf(filter.ReactionTime.HasValue, x => x.m.ReactionTimeId == filter.ReactionTime.Value)
                             .WhereIf(filter.ServiceLocation.HasValue, x => x.m.ServiceLocationId == filter.ServiceLocation.Value);
            }

            query = query.OrderBy(x => x.m.Id);

            var result = query.Select(x => new HwCostDto
            {
                Id = x.m.Id,

                Country = x.m.Country,
                Wg = x.m.Wg,
                Availability = x.m.Availability,
                Duration = x.m.Duration,
                ReactionTime = x.m.ReactionTime,
                ReactionType = x.m.ReactionType,
                ServiceLocation = x.m.ServiceLocation,

                AvailabilityFee = x.hw.AvailabilityFee,
                AvailabilityFee_Approved = x.hw.AvailabilityFee_Approved,

                Credits  = x.hw.Credits,
                Credits_Approved = x.hw.Credits_Approved,

                FieldServiceCost = x.hw.FieldServiceCost,
                FieldServiceCost_Approved = x.hw.FieldServiceCost_Approved,

                HddRetention = x.hw.HddRetention,
                HddRetention_Approved = x.hw.HddRetention_Approved,

                LocalServiceStandardWarranty = x.hw.LocalServiceStandardWarranty,
                LocalServiceStandardWarranty_Approved = x.hw.LocalServiceStandardWarranty_Approved,

                Logistic = x.hw.Logistic,
                Logistic_Approved = x.hw.Logistic_Approved,

                MaterialW = x.hw.MaterialW,
                MaterialW_Approved = x.hw.MaterialW_Approved,

                MaterialOow = x.hw.MaterialOow,
                MaterialOow_Approved = x.hw.MaterialOow_Approved,

                TaxAndDutiesW = x.hw.TaxAndDutiesW,
                TaxAndDutiesW_Approved = x.hw.TaxAndDutiesW_Approved,

                TaxAndDutiesOow = x.hw.TaxAndDutiesOow,
                TaxAndDutiesOow_Approved = x.hw.TaxAndDutiesOow_Approved,

                OtherDirect = x.hw.OtherDirect,
                OtherDirect_Approved = x.hw.OtherDirect_Approved,

                ProActive = x.hw.ProActive,
                ProActive_Approved = x.hw.ProActive_Approved,

                Reinsurance = x.hw.Reinsurance,
                Reinsurance_Approved = x.hw.Reinsurance,

                ServiceSupport = x.hw.ServiceSupport,
                ServiceSupport_Approved = x.hw.ServiceSupport_Approved,

                ServiceTC = x.hw.ServiceTC,
                ServiceTC_Approved = x.hw.ServiceTC_Approved,

                ServiceTCManual = x.hw.ServiceTCManual,
                ServiceTCManual_Approved = x.hw.ServiceTCManual_Approved,

                ServiceTP = x.hw.ServiceTP,
                ServiceTP_Approved = x.hw.ServiceTP_Approved,

                ServiceTPManual = x.hw.ServiceTPManual,
                ServiceTPManual_Approved = x.hw.ServiceTPManual_Approved
            });

            return result.Paging(start, limit, out count);
        }

        public IEnumerable<SwCostDto> GetSoftwareCost(SwFilterDto filter, int start, int limit, out int count)
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
                TransferPrice_Approved = x.TransferPrice_Approved
            });

            return result.Paging(start, limit, out count);
        }

        public void SaveHardwareCost(IEnumerable<HwCostManualDto> records)
        {
            var entities = hwRepo.GetAll()
                                 .Where(x => records.Any(y => y.Id == x.Id) &&
                                             x.Matrix.Country.CanOverrideTransferCostAndPrice)
                                 .ToDictionary(x => x.Id, y => y);

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
