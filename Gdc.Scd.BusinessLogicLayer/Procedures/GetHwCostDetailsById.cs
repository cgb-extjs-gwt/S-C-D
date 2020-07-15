using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Collections.Generic;
using System.Data.Common;

namespace Gdc.Scd.BusinessLogicLayer.Procedures
{
    public class GetHwCostDetailsById
    {
        private const string PROC_NAME = "Hardware.SpGetCostDetailsByID";

        private readonly IRepositorySet _repositorySet;

        public GetHwCostDetailsById(IRepositorySet repositorySet)
        {
            _repositorySet = repositorySet;
        }

        public List<CostDetailDto> Execute(bool approved, long id)
        {
            return _repositorySet.ExecuteProc<CostDetailDto>(PROC_NAME, Prepare(approved, id));
        }

        private static DbParameter[] Prepare(bool approved, long id)
        {
            return new DbParameter[] {
                new DbParameterBuilder().WithName("@approved").WithValue(approved).Build(),
                new DbParameterBuilder().WithName("@id").WithValue(id).Build()
            };
        }

        public class CostDetailDto
        {
            public string CostBlock { get; set; }
            public string CostElement { get; set; }
            public string Dependency { get; set; }
            public string Level { get; set; }
            public string Value { get; set; }
            public bool Mandatory { get; set; }
        }
    }
}
