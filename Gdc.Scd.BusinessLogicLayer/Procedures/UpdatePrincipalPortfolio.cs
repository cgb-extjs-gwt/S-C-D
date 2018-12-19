using Gdc.Scd.BusinessLogicLayer.Dto.Portfolio;
using Gdc.Scd.Core.Entities.Portfolio;
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

        public async Task ExecuteAsync(PortfolioRuleSetDto dto, bool deny)
        {
            var proc = deny ? PROC_DENY_PORTFOLIO : PROC_ALLOW_PORTFOLIO;

            if (dto.IsCorePortfolio)
            {
                await repositorySet.ExecuteProcAsync(proc, Prepare(PortfolioType.CorePortfolio, dto));
            }

            if (dto.IsMasterPortfolio)
            {
                await repositorySet.ExecuteProcAsync(proc, Prepare(PortfolioType.MasterPortfolio, dto));
            }

            if (dto.IsGlobalPortfolio)
            {
                await repositorySet.ExecuteProcAsync(proc, Prepare(PortfolioType.GlobalPortfolio, dto));
            }
        }

        private DbParameter[] Prepare(PortfolioType type, PortfolioRuleSetDto dto)
        {
            return new DbParameter[] {
                new DbParameterBuilder().WithName("@type").WithValue((byte)type).Build(),
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
