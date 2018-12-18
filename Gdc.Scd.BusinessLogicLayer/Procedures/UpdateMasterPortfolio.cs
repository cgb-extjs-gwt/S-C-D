using Gdc.Scd.BusinessLogicLayer.Dto.Portfolio;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Procedures
{
    public class UpdateMasterPortfolio
    {
        const string PROC_NAME = "Matrix.AddRules";

        private readonly IRepositorySet repositorySet;

        public UpdateMasterPortfolio(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public Task ExecuteAsync(PortfolioRuleSetDto dto, bool deny)
        {
            if (dto.CountryId.HasValue)
            {
                return repositorySet.ExecuteProcAsync(PROC_NAME, Prepare(dto));
            }
            else
            {
                throw new ArgumentException("Invalid country");
            }
        }

        private DbParameter[] Prepare(PortfolioRuleSetDto dto)
        {
            return new DbParameter[] {
                new DbParameterBuilder().WithName("@cnt").WithValue(dto.CountryId).Build(),
                new DbParameterBuilder().WithName("@wg").WithListIdValue(dto.Wgs).Build(),
                new DbParameterBuilder().WithName("@av").WithListIdValue(dto.Availabilities).Build(),
                new DbParameterBuilder().WithName("@dur").WithListIdValue(dto.Durations).Build(),
                new DbParameterBuilder().WithName("@rtype").WithListIdValue(dto.ReactionTypes).Build(),
                new DbParameterBuilder().WithName("@rtime").WithListIdValue(dto.ReactionTimes).Build(),
                new DbParameterBuilder().WithName("@loc").WithListIdValue(dto.ServiceLocations).Build()
            };
        }
    }
}
