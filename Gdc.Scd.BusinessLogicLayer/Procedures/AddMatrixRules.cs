using Gdc.Scd.BusinessLogicLayer.Dto.CapabilityMatrix;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Data.Common;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Procedures
{
    public class AddMatrixRules
    {
        const string PROC_NAME = "AddMatrixRules";

        private readonly IRepositorySet repositorySet;

        public AddMatrixRules(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute(CapabilityMatrixRuleSetDto dto)
        {
            repositorySet.ExecuteProc(PROC_NAME, Prepare(dto));
        }

        public Task ExecuteAsync(CapabilityMatrixRuleSetDto dto)
        {
            return repositorySet.ExecuteProcAsync(PROC_NAME, Prepare(dto));
        }

        private DbParameter[] Prepare(CapabilityMatrixRuleSetDto dto)
        {
            return new DbParameter[] {
                new SqlParameterBuilder().WithName("@cnt").WithValue(dto.CountryId).Build(),
                new SqlParameterBuilder().WithName("@wg").WithListIdValue(dto.Wgs).Build(),
                new SqlParameterBuilder().WithName("@av").WithListIdValue(dto.Availabilities).Build(),
                new SqlParameterBuilder().WithName("@dur").WithListIdValue(dto.Durations).Build(),
                new SqlParameterBuilder().WithName("@rtype").WithListIdValue(dto.ReactionTypes).Build(),
                new SqlParameterBuilder().WithName("@rtime").WithListIdValue(dto.ReactionTimes).Build(),
                new SqlParameterBuilder().WithName("@loc").WithListIdValue(dto.ServiceLocations).Build(),
                new SqlParameterBuilder().WithName("@globalPortfolio").WithValue(dto.IsGlobalPortfolio).Build(),
                new SqlParameterBuilder().WithName("@masterPortfolio").WithValue(dto.IsMasterPortfolio).Build(),
                new SqlParameterBuilder().WithName("@corePortfolio").WithValue(dto.IsCorePortfolio).Build()
            };
        }
    }
}
