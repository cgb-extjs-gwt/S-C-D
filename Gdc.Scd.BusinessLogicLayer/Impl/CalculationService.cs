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
            var query = matrixRepo.GetAll();

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

            count = query.Count(); //optimization! get count without join!!!

            var result = from m in query
                         join hw in hwRepo.GetAll() on m.Id equals hw.Id
                         select new HwCostDto
                         {
                             Id = m.Id,

                             Country = m.Country,
                             Wg = m.Wg,
                             Availability = m.Availability,
                             Duration = m.Duration,
                             ReactionTime = m.ReactionTime,
                             ReactionType = m.ReactionType,
                             ServiceLocation = m.ServiceLocation,

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
                         };

            return result.Paging(start, limit);
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
                TransferPrice_Approved = x.TransferPrice_Approved,

                ProActive = x.ProActive,
                ProActive_Approved = x.ProActive_Approved
            });

            return result.Paging(start, limit, out count);
        }

        public void SaveHardwareCost(IEnumerable<HwCostManualDto> records)
        {
            var recordsId = records.Select(x => x.Id);

            var query = hwRepo.GetAll()
                              .Where(x => recordsId.Contains(x.Id) &&
                                          x.Matrix.Country.CanOverrideTransferCostAndPrice);

            var entities = query.ToDictionary(x => x.Id, y => y);

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
