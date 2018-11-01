using Gdc.Scd.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Export.CdCs.Procedures
{
    class GetServiceCostsBySla
    {
        private readonly CommonService _service;

        public GetServiceCostsBySla(CommonService service)
        {
            _service = service;
        }

        public ServiceCostDto Execute(string country, SlaDto sla)
        {
            var procName = Enums.Functions.GetServiceCostsBySla;
            var data = _service.ExecuteAsTable(procName, FillParameters(country, sla));
            var row = data.Rows[0];
            return GetServiceCost(sla.FspCode, row);
        }

        private DbParameter[] FillParameters(string country, SlaDto sla)
        {
            var result = new DbParameter[] {
                _service.FillParameter("cnt", country),
                _service.FillParameter("loc", sla.ServiceLocation),
                _service.FillParameter("av", sla.Availability),
                _service.FillParameter("reactiontime", sla.ReactionTime),
                _service.FillParameter("reactiontype", sla.ReactionType),
                _service.FillParameter("wg", sla.WarrantyGroup),
                _service.FillParameter("dur", sla.Duration)
            };

            return result;
        }

        private ServiceCostDto GetServiceCost(string fspCode, DataRow row)
        {
            var serviceCost = new ServiceCostDto
            {
                Country = row.Field<string>("Country"),
                FspCode = fspCode,
                ServiceTC = row.Field<double>("ServiceTC"),
                ServiceTP = row.Field<double>("ServiceTP"),
                ServiceTP_MonthlyYear1 = 0,
                ServiceTP_MonthlyYear2 = 0,
                ServiceTP_MonthlyYear3 = 0,
                ServiceTP_MonthlyYear4 = 0,
                ServiceTP_MonthlyYear5 = 0
            };

            var serviceTP_Str = row.Field<string>("ServiceTP_Str_Approved");
            if (!String.IsNullOrEmpty(serviceTP_Str))
            {
                var values = serviceTP_Str.Split(';');
                var valuesCount = 0;

                serviceCost.ServiceTP_MonthlyYear1 = valuesCount > 0 ? Convert.ToDouble(values[0]) : 0;
                serviceCost.ServiceTP_MonthlyYear2 = valuesCount > 1 ? Convert.ToDouble(values[1]) : 0;
                serviceCost.ServiceTP_MonthlyYear3 = valuesCount > 2 ? Convert.ToDouble(values[2]) : 0;
                serviceCost.ServiceTP_MonthlyYear4 = valuesCount > 3 ? Convert.ToDouble(values[3]) : 0;
                serviceCost.ServiceTP_MonthlyYear5 = valuesCount > 4 ? Convert.ToDouble(values[4]) : 0;
            }

            return serviceCost;
        }
    }
}
