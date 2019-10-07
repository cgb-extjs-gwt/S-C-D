using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using Gdc.Scd.Export.CdCs.Dto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Gdc.Scd.Export.CdCs.Procedures
{
    class GetHddRetentionCosts
    {
        private const string HDD_RETENTION = "Report.HddRetention";

        private readonly CommonService _service;

        public GetHddRetentionCosts(CommonService service)
        {
            _service = service;
        }

        public List<HddRetentionDto> Execute(string country)
        {
            var data = _service.ExecuteAsTable(HDD_RETENTION, FillParameters(country));
            return GetHddRetentionCost(data);
        }

        private DbParameter[] FillParameters(string country)
        {
            return new DbParameter[] {
                new DbParameterBuilder().WithName("cnt").WithValue(country).Build()
            };
        }

        private List<HddRetentionDto> GetHddRetentionCost(DataTable table)
        {
            var hddRetentionList = new List<HddRetentionDto>();
            if(table != null)
            {
                for (var rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
                {
                    var row = table.Rows[rowIndex];

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
            }
            

            return hddRetentionList.OrderBy(x => x.Wg).ToList();
        }
    }
}
