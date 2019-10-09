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

        private int COUNTRY_GROUP;
        private int KEY;

        private int SERVICELOCATION;
        private int AVAILABILITY;
        private int REACTIONTIME;
        private int REACTIONTYPE;
        private int WG;
        private int DURATION;

        private int SERVICETC;
        private int SERVICETP;
        private int SERVICETP_MONTHLYYEAR1;
        private int SERVICETP_MONTHLYYEAR2;
        private int SERVICETP_MONTHLYYEAR3;
        private int SERVICETP_MONTHLYYEAR4;
        private int SERVICETP_MONTHLYYEAR5;

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
                CountryGroup = reader.GetString(COUNTRY_GROUP),
                Key = reader.GetString(KEY),

                ServiceLocation = reader.GetString(SERVICELOCATION),
                Availability = reader.GetString(AVAILABILITY),
                ReactionTime = reader.GetString(REACTIONTIME),
                ReactionType = reader.GetString(REACTIONTYPE),
                WarrantyGroup = reader.GetString(WG),
                Duration = reader.GetString(DURATION),

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
            COUNTRY_GROUP = reader.GetOrdinal("CountryGroup");
            KEY = reader.GetOrdinal("Key");

            SERVICELOCATION = reader.GetOrdinal("ServiceLocation");
            AVAILABILITY = reader.GetOrdinal("Availability");
            REACTIONTIME = reader.GetOrdinal("ReactionTime");
            REACTIONTYPE = reader.GetOrdinal("ReactionType");
            WG = reader.GetOrdinal("Wg");
            DURATION = reader.GetOrdinal("Duration");

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
