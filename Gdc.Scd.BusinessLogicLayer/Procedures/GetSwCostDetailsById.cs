using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Collections.Generic;
using System.Data.Common;

namespace Gdc.Scd.BusinessLogicLayer.Procedures
{
    public class GetSwCostDetailsById
    {
        private const string PROC_NAME = "SoftwareSolution.SpGetCostDetailsByID";

        private readonly IRepositorySet _repositorySet;

        public GetSwCostDetailsById(IRepositorySet repositorySet)
        {
            _repositorySet = repositorySet;
        }

        public List<GetHwCostDetailsById.CostDetailDto> Execute(bool approved, long id)
        {
            return _repositorySet.ExecuteProc<GetHwCostDetailsById.CostDetailDto>(PROC_NAME, Prepare(approved, id));
        }

        private static DbParameter[] Prepare(bool approved, long id)
        {
            return new DbParameter[] {
                new DbParameterBuilder().WithName("@approved").WithValue(approved).Build(),
                new DbParameterBuilder().WithName("@id").WithValue(id).Build()
            };
        }
    }
}
