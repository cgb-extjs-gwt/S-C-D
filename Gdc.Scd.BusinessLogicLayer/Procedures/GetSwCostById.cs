using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Data.Common;
using System.Linq;

namespace Gdc.Scd.BusinessLogicLayer.Procedures
{
    public class GetSwCostById
    {
        private const string PROC_NAME = "SoftwareSolution.SpGetCostsByID";

        private readonly IRepositorySet _repositorySet;

        public GetSwCostById(IRepositorySet repositorySet)
        {
            _repositorySet = repositorySet;
        }

        public SwCostDto Execute(bool approved, long id)
        {
            var data = _repositorySet.ExecuteProc<SwCostDto>(PROC_NAME, Prepare(approved, id));
            return data != null ? data.First() : null;
        }

        private static DbParameter[] Prepare(bool approved, long id)
        {
            return new DbParameter[] {
                new DbParameterBuilder().WithName("@approved").WithValue(approved).Build(),
                new DbParameterBuilder().WithName("@id").WithValue(id).Build()
            };
        }

        public class SwCostDto
        {
            public long Id { get; set; }
            public string Fsp { get; set; }
            public string SwDigit { get; set; }
            public string Sog { get; set; }
            public string Availability { get; set; }
            public string Duration { get; set; }
            public double InstalledBaseCountry { get; set; }
            public double InstalledBaseSog { get; set; }
            public double TotalInstalledBaseSog { get; set; }
            public double Reinsurance { get; set; }
            public double ServiceSupport { get; set; }
            public double TransferPrice { get; set; }
            public double MaintenanceListPrice { get; set; }
            public double DealerPrice { get; set; }
            public double DiscountDealerPrice { get; set; }
        }

    }
}
