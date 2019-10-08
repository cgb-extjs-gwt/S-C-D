using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using Gdc.Scd.Export.CdCs.Dto;
using System.Collections.Generic;
using System.Data.Common;

namespace Gdc.Scd.Export.CdCs.Procedures
{
    public class GetServiceCostsBySla
    {
        private const string PROC = "Report.GetServiceCostsBySlaTable";

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

        public List<ServiceCostDto> Execute(long country, SlaCollection sla)
        {
            prepared = false;
            return _repo.ExecuteAsList(SelectQuery(), Read, FillParameters(country, sla));
        }

        private string SelectQuery()
        {
            return new SqlStringBuilder().Append("exec ").Append(PROC).Append(" @cnt, @sla").Build();
        }

        private DbParameter[] FillParameters(long country, SlaCollection sla)
        {
            return new[] {
                 new DbParameterBuilder().WithName("cnt").WithValue(country).Build(),
                 new DbParameterBuilder().WithName("sla").WithTypeName("Report.SlaString").WithValue(sla.AsTable()).Build()
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
