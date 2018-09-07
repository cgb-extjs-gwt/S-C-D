using Gdc.Scd.Core.Dto.AvailabilityFee;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Collections.Generic;
using System.Data.Common;

namespace Gdc.Scd.DataAccessLayer.Procedures
{
    public class AvailabilityFeeAdmin
    {
        private const string PROC_NAME = "GetAvailabilityFeeCoverageCombination";

        private readonly IRepositorySet _repositorySet;

        public AvailabilityFeeAdmin(IRepositorySet repositorySet)
        {
            _repositorySet = repositorySet;
        }

        public List<AdminAvailabilityFeeDto> Execute(int pageNumber, int limit, out int totalCount)
        {
            var parameters = Prepare(pageNumber, limit);
            var outParameter = SqlParameterBuilder.CreateOutputParam("@totalCount", System.Data.DbType.Int32);
            return _repositorySet.ExecuteProc<AdminAvailabilityFeeDto, int>(PROC_NAME, outParameter, 
                out totalCount,
                parameters);
        }

        private DbParameter[] Prepare(int pageNumber, int limit)
        {
            return new DbParameter[] {
                 SqlParameterBuilder.Create("@pageSize", limit),
                 SqlParameterBuilder.Create("@pageNumber", pageNumber)
            };
        }
    }
}
