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

        public List<AdminAvailabilityFeeDto> Execute(int pageNumber, int limit, out int totalCount, AdminAvailabilityFeeFilterDto filter = null)
        {
            var parameters = Prepare(pageNumber, limit, filter);
            var result = _repositorySet.ExecuteProc<AdminAvailabilityFeeDto>(PROC_NAME, parameters);
            totalCount = GetTotal(parameters);
            return result;
        }

        private static DbParameter[] Prepare(int pageNumber, int limit, AdminAvailabilityFeeFilterDto filter = null)
        {
            return new DbParameter[] {
                new DbParameterBuilder().WithName("@cnt").WithValue(filter!=null?filter.Country:null).Build(),
                new DbParameterBuilder().WithName("@rtime").WithValue(filter!=null?filter.ReactionTime:null).Build(),
                new DbParameterBuilder().WithName("@rtype").WithValue(filter!=null?filter.ReactionType:null).Build(),
                new DbParameterBuilder().WithName("@serloc").WithValue(filter!=null?filter.ServiceLocation:null).Build(),
                new DbParameterBuilder().WithName("@isapp").WithValue(filter!=null?filter.IsApplicable:null).Build(),

                 new DbParameterBuilder().WithName("@pageSize").WithValue(limit).Build(),
                 new DbParameterBuilder().WithName("@pageNumber").WithValue(pageNumber).Build(),
                 new DbParameterBuilder().WithName("@totalCount").WithType(DbType.Int32).WithDirection(ParameterDirection.Output).Build()
            };
        }

        private static int GetTotal(DbParameter[] parameters)
        {
            return parameters[7].GetInt32();
        }
    }
}
