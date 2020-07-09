using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Collections.Generic;
using System.Data.Common;

namespace Gdc.Scd.BusinessLogicLayer.Procedures
{
    public class GetHwStdwDetailsById
    {
        private const string PROC_NAME = "Hardware.SpGetStdwDetailsByID";

        private readonly IRepositorySet _repositorySet;

        public GetHwStdwDetailsById(IRepositorySet repositorySet)
        {
            _repositorySet = repositorySet;
        }

        public List<GetHwCostDetailsById.CostDetailDto> Execute(bool approved, long cnt, long wg)
        {
            return _repositorySet.ExecuteProc<GetHwCostDetailsById.CostDetailDto>(PROC_NAME, Prepare(approved, cnt, wg));
        }

        private static DbParameter[] Prepare(bool approved, long cnt, long wg)
        {
            return new DbParameter[] {
                new DbParameterBuilder().WithName("@approved").WithValue(approved).Build(),
                new DbParameterBuilder().WithName("@cntID").WithValue(cnt).Build(),
                new DbParameterBuilder().WithName("@wgID").WithValue(wg).Build()
            };
        }
    }
}
