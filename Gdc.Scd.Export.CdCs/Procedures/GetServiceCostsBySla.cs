using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using Gdc.Scd.Export.CdCs.Dto;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Gdc.Scd.Export.CdCs.Procedures
{
    class GetServiceCostsBySla
    {
        private const string GET_SERVICE_COSTS_BY_SLA = "Report.GetServiceCostsBySla";

        private readonly CommonService _repository;

        public GetServiceCostsBySla(CommonService repository)
        {
            _repository = repository;
        }

        public List<ServiceCostDto> Execute(string country, List<SlaDto> slaList)
        {
            var result = new List<ServiceCostDto>();

            foreach (var sla in slaList)
            {
                var data = _repository.ExecuteAsTable(GET_SERVICE_COSTS_BY_SLA, FillParameters(country, sla));
                var row = data != null && data.Rows.Count > 0 ? data.Rows[0] : null;
                result.Add(GetServiceCost(sla, row));
            }

            return result;
        }

        private DbParameter[] FillParameters(string country, SlaDto sla)
        {
            return new[] {
                 new DbParameterBuilder().WithName("cnt").WithValue(country).Build(),
                 new DbParameterBuilder().WithName("loc").WithValue(sla.ServiceLocation).Build(),
                 new DbParameterBuilder().WithName("av").WithValue(sla.Availability).Build(),
                 new DbParameterBuilder().WithName("reactiontime").WithValue(sla.ReactionTime).Build(),
                 new DbParameterBuilder().WithName("reactiontype").WithValue(sla.ReactionType).Build(),
                 new DbParameterBuilder().WithName("wg").WithValue(sla.WarrantyGroup).Build(),
                 new DbParameterBuilder().WithName("dur").WithValue(sla.Duration).Build()
            };
        }

        private ServiceCostDto GetServiceCost(SlaDto sla, DataRow row)
        {
            var serviceCost = new ServiceCostDto
            {
                Sla = sla,
                ServiceTC = CommonService.CheckDoubleField(row, "ServiceTC"),
                ServiceTP = CommonService.CheckDoubleField(row, "ServiceTP"),
                ServiceTP_MonthlyYear1 = CommonService.CheckDoubleField(row, "ServiceTPMonthly1"),
                ServiceTP_MonthlyYear2 = CommonService.CheckDoubleField(row, "ServiceTPMonthly2"),
                ServiceTP_MonthlyYear3 = CommonService.CheckDoubleField(row, "ServiceTPMonthly3"),
                ServiceTP_MonthlyYear4 = CommonService.CheckDoubleField(row, "ServiceTPMonthly4"),
                ServiceTP_MonthlyYear5 = CommonService.CheckDoubleField(row, "ServiceTPMonthly5")

            };
            return serviceCost;
        }
    }
}
