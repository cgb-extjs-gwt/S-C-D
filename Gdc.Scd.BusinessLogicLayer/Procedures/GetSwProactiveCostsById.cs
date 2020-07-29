using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Data.Common;
using System.Linq;

namespace Gdc.Scd.BusinessLogicLayer.Procedures
{
    public class GetSwProactiveCostsById
    {
        private const string PROC_NAME = "SoftwareSolution.SpGetProActiveCostsByID";

        private readonly IRepositorySet _repositorySet;

        public GetSwProactiveCostsById(IRepositorySet repositorySet)
        {
            _repositorySet = repositorySet;
        }

        public SwProactiveCostDto Execute(bool approved, long id, string fsp)
        {
            var data = _repositorySet.ExecuteProc<SwProactiveCostDto>(PROC_NAME, Prepare(approved, id, fsp));
            return data != null ? data.First() : null;
        }

        private static DbParameter[] Prepare(bool approved, long id, string fsp)
        {
            return new DbParameter[] {
                new DbParameterBuilder().WithName("@approved").WithValue(approved).Build(),
                new DbParameterBuilder().WithName("@id").WithValue(id).Build(),
                new DbParameterBuilder().WithName("@fsp").WithValue(fsp).Build()
            };
        }

        public class SwProactiveCostDto
        {
            public long Id { get; set; }
            public string Fsp { get; set; }
            public string Country { get; set; }
            public string Sog { get; set; }
            public string SwDigit { get; set; }
            public string Availability { get; set; }
            public string Year { get; set; }
            public string ProactiveSla { get; set; }
            public double? ProActive { get; set; }
        }
    }
}
