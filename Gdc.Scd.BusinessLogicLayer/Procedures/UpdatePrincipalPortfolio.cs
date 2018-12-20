using Gdc.Scd.BusinessLogicLayer.Dto.Portfolio;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Data.Common;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Procedures
{
    public class UpdatePrincipalPortfolio
    {
        const string PROC_DENY_PORTFOLIO = "Portfolio.DenyPrincipalPortfolio";
        const string PROC_ALLOW_PORTFOLIO = "Portfolio.AllowPrincipalPortfolio";

        private readonly IRepositorySet repositorySet;

        public UpdatePrincipalPortfolio(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public Task UpdateAsync(PortfolioRuleSetDto dto, bool deny)
        {
            var proc = deny ? PROC_DENY_PORTFOLIO : PROC_ALLOW_PORTFOLIO;
            var valid = dto.IsGlobalPortfolio || dto.IsMasterPortfolio || dto.IsCorePortfolio;

            if (!valid)
            {
                throw new System.ArgumentException("No portfolio specified");
            }

            return repositorySet.ExecuteProcAsync(proc, Prepare(dto));
        }

        private DbParameter[] Prepare(PortfolioRuleSetDto dto)
        {
            return new DbParameter[] {
                new DbParameterBuilder().WithName("@wg").WithListIdValue(dto.Wgs).Build(),
                new DbParameterBuilder().WithName("@av").WithListIdValue(dto.Availabilities).Build(),
                new DbParameterBuilder().WithName("@dur").WithListIdValue(dto.Durations).Build(),
                new DbParameterBuilder().WithName("@rtype").WithListIdValue(dto.ReactionTypes).Build(),
                new DbParameterBuilder().WithName("@rtime").WithListIdValue(dto.ReactionTimes).Build(),
                new DbParameterBuilder().WithName("@loc").WithListIdValue(dto.ServiceLocations).Build(),
                new DbParameterBuilder().WithName("@pro").WithListIdValue(dto.ProActives).Build(),
                new DbParameterBuilder().WithName("@globalPortfolio").WithValue(dto.IsGlobalPortfolio).Build(),
                new DbParameterBuilder().WithName("@masterPortfolio").WithValue(dto.IsMasterPortfolio).Build(),
                new DbParameterBuilder().WithName("@corePortfolio").WithValue(dto.IsCorePortfolio).Build()
            };
        }
    }
}
