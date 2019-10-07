using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using Gdc.Scd.Export.CdCs.Dto;
using System.Collections.Generic;
using System.Data.Common;

namespace Gdc.Scd.Export.CdCs.Procedures
{
    class GetServiceCostsBySla
    {
        private const string PROC = "Report.GetServiceCostsBySla";

        private readonly IRepositorySet _repo;

        private bool prepared;

        private int SERVICETC;
        private int SERVICETP;
        private int SERVICETP_MONTHLYYEAR1;
        private int SERVICETP_MONTHLYYEAR2;
        private int SERVICETP_MONTHLYYEAR3;
        private int SERVICETP_MONTHLYYEAR4;
        private int SERVICETP_MONTHLYYEAR5;

        private SlaDto current;

        public GetServiceCostsBySla(IRepositorySet repo)
        {
            _repo = repo;
        }

        public List<ServiceCostDto> Execute(string country, List<SlaDto> slaList)
        {
            var result = new List<ServiceCostDto>(100);

            foreach (var sla in slaList)
            {
                result.AddRange(Execute(country, sla));
            }

            return result;
        }

        public List<ServiceCostDto> Execute(string country, SlaDto sla)
        {
            prepared = false;
            current = sla;

            var parameters = FillParameters(country, sla);
            var sql = SelectQuery(parameters);

            return _repo.ExecuteAsList(sql, Read, parameters);
        }

        private string SelectQuery(DbParameter[] parameters)
        {
            return new SqlStringBuilder()
                   .Append("SELECT * FROM ").AppendFunc(PROC, parameters)
                   .Build();
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

        private ServiceCostDto Read(DbDataReader reader)
        {
            if (!prepared)
            {
                Prepare(reader);
            }

            return new ServiceCostDto
            {
                Sla = current,
                ServiceTC = reader.GetDoubleOrDefault(SERVICETC),
                ServiceTP = reader.GetDoubleOrDefault(SERVICETP),
                ServiceTP_MonthlyYear1 = reader.GetDoubleOrDefault(SERVICETP_MONTHLYYEAR1),
                ServiceTP_MonthlyYear2 = reader.GetDoubleOrDefault(SERVICETP_MONTHLYYEAR2),
                ServiceTP_MonthlyYear3 = reader.GetDoubleOrDefault(SERVICETP_MONTHLYYEAR3),
                ServiceTP_MonthlyYear4 = reader.GetDoubleOrDefault(SERVICETP_MONTHLYYEAR4),
                ServiceTP_MonthlyYear5 = reader.GetDoubleOrDefault(SERVICETP_MONTHLYYEAR5)
            };
        }

        private void Prepare(DbDataReader reader)
        {
            SERVICETC = reader.GetOrdinal("ServiceTC");
            SERVICETP = reader.GetOrdinal("ServiceTP");
            SERVICETP_MONTHLYYEAR1 = reader.GetOrdinal("ServiceTPMonthly1");
            SERVICETP_MONTHLYYEAR2 = reader.GetOrdinal("ServiceTPMonthly2");
            SERVICETP_MONTHLYYEAR3 = reader.GetOrdinal("ServiceTPMonthly3");
            SERVICETP_MONTHLYYEAR4 = reader.GetOrdinal("ServiceTPMonthly4");
            SERVICETP_MONTHLYYEAR5 = reader.GetOrdinal("ServiceTPMonthly5");
            //
            prepared = true;
        }
    }
}
