using Gdc.Scd.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Export.CdCs.Procedures
{
    class GetServiceCostsBySla
    {
        private readonly IRepositorySet _repo;


        public GetServiceCostsBySla(IRepositorySet repo)
        {
            _repo = repo;
        }

        public ServiceCostDto GetServiceCosts(string country, SlaDto sla)
        {
            var procName = Enums.Functions.GetServiceCostsBySla;
            var data = ExecuteAsTable(procName, FillParameters(country, sla));
            var row = data.Rows[0];
            return GetServiceCost(sla.FspCode, row);
        }

        private DbParameter[] FillParameters(string country, SlaDto sla)
        {
            var result = new DbParameter[] {
                FillParameter("cnt", country),
                FillParameter("loc", sla.ServiceLocation),
                FillParameter("av", sla.Availability),
                FillParameter("reactiontime", sla.ReactionTime),
                FillParameter("reactiontype", sla.ReactionType),
                FillParameter("wg", sla.WarrantyGroup),
                FillParameter("dur", sla.Duration)
            };

            return result;
        }
    }
}
