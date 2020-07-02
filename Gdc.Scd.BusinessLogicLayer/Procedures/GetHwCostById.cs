using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Data.Common;
using System.Linq;

namespace Gdc.Scd.BusinessLogicLayer.Procedures
{
    public class GetHwCostById
    {
        private const string PROC_NAME = "Hardware.SpGetCostsByID";

        private readonly IRepositorySet _repositorySet;

        public GetHwCostById(IRepositorySet repositorySet)
        {
            _repositorySet = repositorySet;
        }

        public HwCostDto Execute(bool approved, long id)
        {
            var data = _repositorySet.ExecuteProc<HwCostDto>(PROC_NAME, Prepare(approved, id));
            return data != null ? data.First() : null;
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
