using Gdc.Scd.BusinessLogicLayer.Dto.CapabilityMatrix;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Data.Common;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Procedures
{
    public class AddMatrixMasterRules
    {
        const string PROC_NAME = "Matrix.AddMasterRules";

        private readonly IRepositorySet repositorySet;

        public AddMatrixMasterRules(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public Task ExecuteAsync(CapabilityMatrixRuleSetDto dto)
        {
            if (dto.IsCorePortfolio || dto.IsMasterPortfolio || dto.IsGlobalPortfolio)
            {
                return repositorySet.ExecuteProcAsync(PROC_NAME, Prepare(dto));
            }
            else
            {
                throw new System.ArgumentException("Invalid portfolio, no portfolio specified");
            }
        }

        private DbParameter[] Prepare(CapabilityMatrixRuleSetDto dto)
        {
            return new DbParameter[] {
                new DbParameterBuilder().WithName("@wg").WithListIdValue(dto.Wgs).Build(),
                new DbParameterBuilder().WithName("@av").WithListIdValue(dto.Availabilities).Build(),
                new DbParameterBuilder().WithName("@dur").WithListIdValue(dto.Durations).Build(),
                new DbParameterBuilder().WithName("@rtype").WithListIdValue(dto.ReactionTypes).Build(),
                new DbParameterBuilder().WithName("@rtime").WithListIdValue(dto.ReactionTimes).Build(),
                new DbParameterBuilder().WithName("@loc").WithListIdValue(dto.ServiceLocations).Build(),
                new DbParameterBuilder().WithName("@globalPortfolio").WithValue(dto.IsGlobalPortfolio).Build(),
                new DbParameterBuilder().WithName("@masterPortfolio").WithValue(dto.IsMasterPortfolio).Build(),
                new DbParameterBuilder().WithName("@corePortfolio").WithValue(dto.IsCorePortfolio).Build()
            };
        }


    }
}
