using Gdc.Scd.BusinessLogicLayer.Dto.Portfolio;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Procedures
{
    public class UpdateLocalPortfolio
    {
        const string PROC_DENY_PORTFOLIO = "Portfolio.DenyLocalPortfolio";
        const string PROC_DENY_PORTFOLIO_BY_ID = "Portfolio.DenyLocalPortfolioById";
        const string PROC_ALLOW_PORTFOLIO = "Portfolio.AllowLocalPortfolio";

        private readonly IRepositorySet repositorySet;

        public UpdateLocalPortfolio(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public Task UpdateAsync(PortfolioRuleSetDto dto, bool deny)
        {
            if (!dto.IsLocalPortfolio())
            {
                throw new ArgumentException("Invalid country");
            }

            var proc = deny ? PROC_DENY_PORTFOLIO : PROC_ALLOW_PORTFOLIO;

            return repositorySet.ExecuteProcAsync(proc, Prepare(dto));
        }

        public Task DenyAsync(long[] ids)
        {
            return repositorySet.ExecuteProcAsync(PROC_DENY_PORTFOLIO_BY_ID, Prepare(ids));
        }

        private DbParameter[] Prepare(long[] ids)
        {
            return new DbParameter[] {
                new DbParameterBuilder().WithName("@ids").WithListIdValue(ids).Build()
            };
        }

        private DbParameter[] Prepare(PortfolioRuleSetDto dto)
        {
            return new DbParameter[] {
                new DbParameterBuilder().WithName("@cnt").WithType(System.Data.DbType.Int64).WithValue(dto.CountryId).Build(),
                new DbParameterBuilder().WithName("@wg").WithListIdValue(dto.Wgs).Build(),
                new DbParameterBuilder().WithName("@av").WithListIdValue(dto.Availabilities).Build(),
                new DbParameterBuilder().WithName("@dur").WithListIdValue(dto.Durations).Build(),
                new DbParameterBuilder().WithName("@rtype").WithListIdValue(dto.ReactionTypes).Build(),
                new DbParameterBuilder().WithName("@rtime").WithListIdValue(dto.ReactionTimes).Build(),
                new DbParameterBuilder().WithName("@loc").WithListIdValue(dto.ServiceLocations).Build(),
                new DbParameterBuilder().WithName("@pro").WithListIdValue(dto.ProActives).Build()
            };
        }
    }
}
