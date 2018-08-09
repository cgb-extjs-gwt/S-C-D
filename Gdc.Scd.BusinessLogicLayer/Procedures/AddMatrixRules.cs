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
                 SqlParameterBuilder.Create("@cnt", dto.CountryId),
                 SqlParameterBuilder.CreateListID("@wg", dto.Wgs),
                 SqlParameterBuilder.CreateListID("@av", dto.Availabilities),
                 SqlParameterBuilder.CreateListID("@dur", dto.Durations),
                 SqlParameterBuilder.CreateListID("@rtype", dto.ReactionTypes),
                 SqlParameterBuilder.CreateListID("@rtime", dto.ReactionTimes),
                 SqlParameterBuilder.CreateListID("@loc", dto.ServiceLocations),
                 SqlParameterBuilder.Create("@globalPortfolio", dto.IsGlobalPortfolio),
                 SqlParameterBuilder.Create("@masterPortfolio", dto.IsMasterPortfolio),
                 SqlParameterBuilder.Create("@corePortfolio", dto.IsCorePortfolio)
            };
        }
    }
}
