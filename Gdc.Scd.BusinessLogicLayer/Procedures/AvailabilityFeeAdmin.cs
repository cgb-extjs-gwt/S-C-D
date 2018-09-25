using Gdc.Scd.BusinessLogicLayer.Dto.AvailabilityFee;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Gdc.Scd.BusinessLogicLayer.Procedures
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
            var result = _repositorySet.ExecuteProc<AdminAvailabilityFeeDto>(PROC_NAME, parameters);
            totalCount = GetTotal(parameters);
            return result;
        }

        private static DbParameter[] Prepare(int pageNumber, int limit)
        {
            return new DbParameter[] {
                 new DbParameterBuilder().WithName("@pageSize").WithValue(limit).Build(),
                 new DbParameterBuilder().WithName("@pageNumber").WithValue(pageNumber).Build(),
                 new DbParameterBuilder().WithName("@totalCount").WithType(DbType.Int32).WithDirection(ParameterDirection.Output).Build()
            };
        }

        private static int GetTotal(DbParameter[] parameters)
        {
            return parameters[2].GetInt32();
        }
    }
}
