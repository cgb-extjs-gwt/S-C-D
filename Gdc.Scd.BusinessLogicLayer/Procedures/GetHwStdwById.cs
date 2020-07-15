using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Data.Common;
using System.Linq;

namespace Gdc.Scd.BusinessLogicLayer.Procedures
{
    public class GetHwStdwById
    {
        private const string PROC_NAME = "Hardware.SpGetStdwByID";

        private readonly IRepositorySet _repositorySet;

        public GetHwStdwById(IRepositorySet repositorySet)
        {
            _repositorySet = repositorySet;
        }

        public HwStdwDto Execute(bool approved, long cnt, long wg)
        {
            var data = _repositorySet.ExecuteProc<HwStdwDto>(PROC_NAME, Prepare(approved, cnt, wg));
            return data != null ? data.FirstOrDefault() : null;
        }

        private static DbParameter[] Prepare(bool approved, long cnt, long wg)
        {
            return new DbParameter[] {
                new DbParameterBuilder().WithName("@approved").WithValue(approved).Build(),
                new DbParameterBuilder().WithName("@cntID").WithValue(cnt).Build(),
                new DbParameterBuilder().WithName("@wgID").WithValue(wg).Build()
            };
        }

        public class HwStdwDto
        {
            public long CountryId { get; set; }
            public string Country { get; set; }
            public long CurrencyId { get; set; }
            public string Currency { get; set; }
            public long ClusterRegionId { get; set; }
            public double ExchangeRate { get; set; }
            public long WgId { get; set; }
            public string Wg { get; set; }
            public long SogId { get; set; }
            public string Sog { get; set; }
            public long ClusterPlaId { get; set; }
            public long RoleCodeId { get; set; }
            public long StdFspId { get; set; }
            public string StdFsp { get; set; }
            public int StdWarranty { get; set; }
            public string StdWarrantyLocation { get; set; }
            public string Availability { get; set; }
            public string Duration { get; set; }
            public string ReactionTime { get; set; }
            public string ReactionType { get; set; }
            public string ServiceLocation { get; set; }
            public string ProActiveSla { get; set; }
            public double AFR1 { get; set; }
            public double AFR2 { get; set; }
            public double AFR3 { get; set; }
            public double AFR4 { get; set; }
            public double AFR5 { get; set; }
            public double AFRP1 { get; set; }
            public double OnsiteHourlyRates { get; set; }
            public bool CanOverrideTransferCostAndPrice { get; set; }
            public double LocalRemoteAccessSetup { get; set; }
            public double LocalRegularUpdate { get; set; }
            public double LocalPreparation { get; set; }
            public double LocalRemoteCustomerBriefing { get; set; }
            public double LocalOnsiteCustomerBriefing { get; set; }
            public double Travel { get; set; }
            public double CentralExecutionReport { get; set; }
            public double Fee { get; set; }
            public double MatW1 { get; set; }
            public double MatW2 { get; set; }
            public double MatW3 { get; set; }
            public double MatW4 { get; set; }
            public double MatW5 { get; set; }
            public double MaterialW { get; set; }
            public double MatOow1 { get; set; }
            public double MatOow2 { get; set; }
            public double MatOow3 { get; set; }
            public double MatOow4 { get; set; }
            public double MatOow5 { get; set; }
            public double MatOow1p { get; set; }
            public double MatCost1 { get; set; }
            public double MatCost2 { get; set; }
            public double MatCost3 { get; set; }
            public double MatCost4 { get; set; }
            public double MatCost5 { get; set; }
            public double MatCost1P { get; set; }
            public double TaxW1 { get; set; }
            public double TaxW2 { get; set; }
            public double TaxW3 { get; set; }
            public double TaxW4 { get; set; }
            public double TaxW5 { get; set; }
            public double TaxAndDutiesW { get; set; }
            public double TaxOow1 { get; set; }
            public double TaxOow2 { get; set; }
            public double TaxOow3 { get; set; }
            public double TaxOow4 { get; set; }
            public double TaxOow5 { get; set; }
            public double TaxOow1P { get; set; }
            public double TaxAndDuties1 { get; set; }
            public double TaxAndDuties2 { get; set; }
            public double TaxAndDuties3 { get; set; }
            public double TaxAndDuties4 { get; set; }
            public double TaxAndDuties5 { get; set; }
            public double TaxAndDuties1P { get; set; }
            public double ServiceSupportPerYear { get; set; }
            public double ServiceSupportPerYearWithoutSar { get; set; }
            public double ServiceSupportW { get; set; }
            public double FieldServiceW { get; set; }
            public double LogisticW { get; set; }
            public double MarkupStandardWarranty { get; set; }
            public double MarkupFactorStandardWarranty { get; set; }
            public double LocalServiceStandardWarranty { get; set; }
            public double LocalServiceStandardWarrantyWithoutSar { get; set; }
            public double LocalServiceStandardWarrantyManual { get; set; }
            public double RiskFactorStandardWarranty { get; set; }
            public double RiskStandardWarranty { get; set; }
            public double Credit1 { get; set; }
            public double Credit2 { get; set; }
            public double Credit3 { get; set; }
            public double Credit4 { get; set; }
            public double Credit5 { get; set; }
            public double Credits { get; set; }
            public double Credit1WithoutSar { get; set; }
            public double Credit2WithoutSar { get; set; }
            public double Credit3WithoutSar { get; set; }
            public double Credit4WithoutSar { get; set; }
            public double Credit5WithoutSar { get; set; }
            public double CreditsWithoutSar { get; set; }

        }
    }
}
