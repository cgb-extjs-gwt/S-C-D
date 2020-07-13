using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Data.Common;
using System.Linq;

namespace Gdc.Scd.BusinessLogicLayer.Procedures
{
    public class GetHwCostById
    {
        private const string PROC_NAME = "Hardware.SpGetCostsByID";

        private readonly IRepositorySet _repositorySet;

        public GetHwCostById(IRepositorySet repositorySet)
        {
            _repositorySet = repositorySet;
        }

        public HwCostDto Execute(bool approved, long id)
        {
            var data = _repositorySet.ExecuteProc<HwCostDto>(PROC_NAME, Prepare(approved, id));
            return data != null ? data.First() : null;
        }

        private static DbParameter[] Prepare(bool approved, long id)
        {
            return new DbParameter[] {
                new DbParameterBuilder().WithName("@approved").WithValue(approved).Build(),
                new DbParameterBuilder().WithName("@id").WithValue(id).Build()
            };
        }

        public class HwCostDto
        {
            public long Id { get; set; }
            public string Fsp { get; set; }
            public long CountryId { get; set; }
            public string Country { get; set; }
            public long CurrencyId { get; set; }
            public string Currency { get; set; }
            public double? ExchangeRate { get; set; }
            public long SogId { get; set; }
            public string Sog { get; set; }
            public long WgId { get; set; }
            public string Wg { get; set; }
            public long AvailabilityId { get; set; }
            public string Availability { get; set; }
            public long DurationId { get; set; }
            public string Duration { get; set; }
            public int Year { get; set; }
            public bool IsProlongation { get; set; }
            public long ReactionTimeId { get; set; }
            public string ReactionTime { get; set; }
            public long ReactionTypeId { get; set; }
            public string ReactionType { get; set; }
            public long ServiceLocationId { get; set; }
            public string ServiceLocation { get; set; }
            public long ProActiveSlaId { get; set; }
            public string ProActiveSla { get; set; }
            public int StdWarranty { get; set; }
            public string StdWarrantyLocation { get; set; }
            public double? AvailabilityFee { get; set; }
            public double? TaxAndDutiesW { get; set; }
            public double? TaxAndDutiesOow { get; set; }
            public double? Reinsurance { get; set; }
            public double? ProActive { get; set; }
            public double? ServiceSupportCost { get; set; }
            public double? MaterialW { get; set; }
            public double? MaterialOow { get; set; }
            public double? FieldServiceCost { get; set; }
            public double? Logistic { get; set; }
            public double? OtherDirect { get; set; }
            public double? LocalServiceStandardWarranty { get; set; }
            public double? LocalServiceStandardWarrantyManual { get; set; }
            public double? LocalServiceStandardWarrantyWithRisk { get; set; }
            public double? Credits { get; set; }
            public double? ReActiveTC { get; set; }
            public double? ServiceTC { get; set; }
            public double? ReActiveTP { get; set; }
            public double? ServiceTP { get; set; }
            public double? ServiceTC1 { get; set; }
            public double? ServiceTC2 { get; set; }
            public double? ServiceTC3 { get; set; }
            public double? ServiceTC4 { get; set; }
            public double? ServiceTC5 { get; set; }
            public double? ServiceTC1P { get; set; }
            public double? ServiceTP1 { get; set; }
            public double? ServiceTP2 { get; set; }
            public double? ServiceTP3 { get; set; }
            public double? ServiceTP4 { get; set; }
            public double? ServiceTP5 { get; set; }
            public double? ServiceTP1P { get; set; }
            public double? ListPrice { get; set; }
            public double? DealerDiscount { get; set; }
            public double? DealerPrice { get; set; }
            public double? ServiceTCManual { get; set; }
            public double? ReActiveTPManual { get; set; }
            public double? ServiceTPManual { get; set; }
            public double? ServiceTCResult { get; set; }
            public double? ReActiveTPResult { get; set; }
            public double? ServiceTPResult { get; set; }
            public double? ServiceTP_Released { get; set; }
        }
    }
}
