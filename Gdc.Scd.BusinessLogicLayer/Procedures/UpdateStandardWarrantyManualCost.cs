using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System.Data;
using System.Data.Common;

namespace Gdc.Scd.BusinessLogicLayer.Procedures
{
    public class UpdateStandardWarrantyManualCost
    {
        private const string PROC = "Hardware.SpUpdateStandardWarrantyManualCost";

        private readonly IRepositorySet _repo;

        public UpdateStandardWarrantyManualCost(IRepositorySet repo)
        {
            _repo = repo;
        }

        public void Execute(long userId, HwCostDto[] items)
        {
            _repo.ExecuteProc(PROC, Prepare(userId, items));
        }

        private static DbParameter[] Prepare(long userId, HwCostDto[] items)
        {
            var pUsr = new DbParameterBuilder().WithName("@usr").WithValue(userId);
            var pCost = new DbParameterBuilder().WithName("@cost");

            var tbl = new DataTable();
            tbl.Columns.Add("Country", typeof(string));
            tbl.Columns.Add("Wg", typeof(string));
            tbl.Columns.Add("StandardWarranty", typeof(double));

            var rows = tbl.Rows;
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                rows.Add(item.Country, item.Wg, item.LocalServiceStandardWarrantyManual);
            }

            pCost.WithTypeName("Hardware.StdwCost").WithValue(tbl);

            return new DbParameter[] {
                 pUsr.Build(),
                 pCost.Build()
            };
        }
    }
}
