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
            var row = data != null && data.Rows.Count > 0 ? data.Rows[0] : null;
            return GetServiceCost(country, sla.FspCode, row);
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

        private ServiceCostDto GetServiceCost(string country, string fspCode, DataRow row)
        {
            var serviceCost = new ServiceCostDto
            {
                Country = country,
                FspCode = fspCode,
                ServiceTC = _service.CheckDoubleField(row, "ServiceTC"),
                ServiceTP = _service.CheckDoubleField(row, "ServiceTP"),
                ServiceTP_MonthlyYear1 = _service.CheckDoubleField(row, "ServiceTP1"),
                ServiceTP_MonthlyYear2 = _service.CheckDoubleField(row, "ServiceTP2"),
                ServiceTP_MonthlyYear3 = _service.CheckDoubleField(row, "ServiceTP3"),
                ServiceTP_MonthlyYear4 = _service.CheckDoubleField(row, "ServiceTP4"),
                ServiceTP_MonthlyYear5 = _service.CheckDoubleField(row, "ServiceTP5")

            };
            return serviceCost;
        }     
    }
}
