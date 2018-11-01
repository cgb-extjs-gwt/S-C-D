using Gdc.Scd.Export.CdCs.Dto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Export.CdCs.Procedures
{
    class GetHddRetentionCosts
    {
        private readonly CommonService _service;

        public GetHddRetentionCosts(CommonService service)
        {
            _service = service;
        }

        public List<HddRetentionDto> Execute()
        {
            var data = _service.ExecuteAsTable(Enums.Functions.HddRetention, FillParameters());
            return GetHddRetentionCost(data);
        }

        private DbParameter[] FillParameters()
        {
            var result = new DbParameter[] { };

            return result;
        }

        private List<HddRetentionDto> GetHddRetentionCost(DataTable table)
        {
            var hddRetentionList = new List<HddRetentionDto>();

            for (var rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
            {
                var row = table.Rows[rowIndex];

                var TransferPrice = Convert.ToDouble(row["TP"]);

                var hddRetentionDto = new HddRetentionDto
                {
                    Wg = row.Field<string>("Wg"),
                    WgName = row.Field<string>("WgDescription"),
                    TransferPrice = Convert.ToDouble(row["TP"]),
                    DealerPrice = Convert.ToDouble(row["DealerPrice"]),
                    ListPrice = Convert.ToDouble(row["ListPrice"])
                };

                hddRetentionList.Add(hddRetentionDto);
            }

            return hddRetentionList.OrderBy(x => x.Wg).ToList();
        }
    }
}
