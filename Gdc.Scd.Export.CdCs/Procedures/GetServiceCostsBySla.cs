using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.Export.CdCs.Dto;

namespace Gdc.Scd.Export.CdCs.Procedures
{
    class GetServiceCostsBySla
    {
        private readonly CommonService _repository;

        public GetServiceCostsBySla(CommonService repository)
        {
            _repository = repository;
        }

        public List<ServiceCostDto> Execute(string country, List<SlaDto> slaList)
        {
            var result = new List<ServiceCostDto>();
            var procName = Enums.Enums.Functions.GetServiceCostsBySla;

            foreach (var sla in slaList)
            {
                var data = _repository.ExecuteAsTable(procName, FillParameters(country, sla));
                var row = data != null && data.Rows.Count > 0 ? data.Rows[0] : null;
                result.Add(GetServiceCost(sla, row));
            }
           
            return result;
        }

        private DbParameter[] FillParameters(string country, SlaDto sla)
        {
            var cnt = new DbParameterBuilder().WithName("cnt").WithValue(country);
            var loc = new DbParameterBuilder().WithName("loc").WithValue(sla.ServiceLocation);
            var av = new DbParameterBuilder().WithName("av").WithValue(sla.Availability);
            var reactionTime = new DbParameterBuilder().WithName("reactiontime").WithValue(sla.ReactionTime);
            var reactionType = new DbParameterBuilder().WithName("reactiontype").WithValue(sla.ReactionType);
            var wg = new DbParameterBuilder().WithName("wg").WithValue(sla.WarrantyGroup);
            var dur = new DbParameterBuilder().WithName("dur").WithValue(sla.Duration);

            var result = new[] {
                cnt.Build(),
                loc.Build(),
                av.Build(),
                reactionTime.Build(),
                reactionType.Build(),
                wg.Build(),
                dur.Build()
            };

            return result;
        }

        private ServiceCostDto GetServiceCost(SlaDto sla, DataRow row)
        {
            var serviceCost = new ServiceCostDto
            {
                Sla = sla,
                ServiceTC = CommonService.CheckDoubleField(row, "ServiceTC"),
                ServiceTP = CommonService.CheckDoubleField(row, "ServiceTP"),
                ServiceTP_MonthlyYear1 = CommonService.CheckDoubleField(row, "ServiceTP1"),
                ServiceTP_MonthlyYear2 = CommonService.CheckDoubleField(row, "ServiceTP2"),
                ServiceTP_MonthlyYear3 = CommonService.CheckDoubleField(row, "ServiceTP3"),
                ServiceTP_MonthlyYear4 = CommonService.CheckDoubleField(row, "ServiceTP4"),
                ServiceTP_MonthlyYear5 = CommonService.CheckDoubleField(row, "ServiceTP5")

            };
            return serviceCost;
        }     
    }
}
