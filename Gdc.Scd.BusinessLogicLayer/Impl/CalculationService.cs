using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
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

        private readonly IRepository<HardwareCalculationResult> hwRepo;

        private readonly IRepository<CapabilityMatrix> matrixRepo;

        private readonly IRepository<SoftwareCalculationResult> swRepo;

        public CalculationService(
                IRepositorySet repositorySet,
                IRepository<HardwareCalculationResult> hwRepo,
                IRepository<SoftwareCalculationResult> swRepo,
                IRepository<CapabilityMatrix> matrixRepo
            )
        {
            this.repositorySet = repositorySet;
            this.hwRepo = hwRepo;
            this.swRepo = swRepo;
            this.matrixRepo = matrixRepo;
        }

        public async Task<Tuple<HwCostDto[], int>> GetHardwareCost(HwFilterDto filter, int start, int limit)
        {
            var query = matrixRepo.GetAll();

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

            var count = await query.GetCountAsync(); //optimization! get count without join!!!

            var query2 = from m in query
                         join hw in hwRepo.GetAll() on m.Id equals hw.Id
                         select new HwCostDto
                         {
                             Id = m.Id,

                             Country = m.Country.Name,
                             Wg = m.Wg.Name,
                             Availability = m.Availability.Name,
                             Duration = m.Duration.Name,
                             ReactionTime = m.ReactionTime.Name,
                             ReactionType = m.ReactionType.Name,
                             ServiceLocation = m.ServiceLocation.Name,

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
                             ServiceTPManual_Approved = hw.ServiceTPManual_Approved,

                             ListPrice = hw.ListPrice,
                             ListPrice_Approved = hw.ListPrice_Approved,

                             DealerDiscount = hw.DealerDiscount,
                             DealerDiscount_Approved = hw.DealerDiscount_Approved,

                             DealerPrice = hw.DealerPrice,
                             DealerPrice_Approved = hw.DealerPrice_Approved
                         };

            var result = await query2.PagingAsync(start, limit);

            return new Tuple<HwCostDto[], int>(result, count);
        }

        public async Task<Tuple<SwCostDto[], int>> GetSoftwareCost(SwFilterDto filter, int start, int limit)
        {
            var query = swRepo.GetAll();

            if (filter != null)
            {
                query = query.WhereIf(filter.Sog.HasValue, x => x.Sog.Id == filter.Sog.Value)
                             .WhereIf(filter.Availability.HasValue, x => x.Availability.Id == filter.Availability.Value)
                             .WhereIf(filter.Year.HasValue, x => x.Year.Id == filter.Year.Value);
            }

            query = query.OrderBy(x => x.Id);

            var count = await query.GetCountAsync();

            var result = await query.Select(x => new SwCostDto
            {
                Sog = x.Sog.Name,
                Availability = x.Availability.Name,
                Year = x.Year.Name,

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
            }).PagingAsync(start, limit);

            return new Tuple<SwCostDto[], int>(result, count);
        }

        public void SaveHardwareCost(IEnumerable<HwCostManualDto> records)
        {
            var recordsId = records.Select(x => x.Id);

            var entities = hwRepo.GetAll()
                              .Where(x => recordsId.Contains(x.Id))
                              .Select(x => new { Hw = x, Country = x.Matrix.Country })
                              .ToDictionary(x => x.Hw.Id, y => y);

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
                    var hw = e.Hw;

                    if (country.CanOverrideTransferCostAndPrice)
                    {
                        hw.ServiceTCManual = rec.ServiceTC;
                        hw.ServiceTPManual = rec.ServiceTP;
                        //
                        hwRepo.Save(hw);
                    }

                    if (country.CanStoreListAndDealerPrices)
                    {
                        hw.ListPrice = rec.ListPrice;
                        hw.DealerDiscount = rec.DealerDiscount;
                        //
                        hwRepo.Save(hw);
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
