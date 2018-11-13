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
    class GetProActiveCosts
    {
        private readonly CommonService _service;

        public GetProActiveCosts(CommonService service)
        {
            _service = service;
        }

        public List<ProActiveDto> Execute(string country)
        {
            var data = _service.ExecuteAsTable(Enums.Functions.GetProActiveByCountryAndWg, FillParameters(country));
            return GetProActiveCost(data);
        }

        private DbParameter[] FillParameters(string country)
        {
            var result = new DbParameter[] {
                _service.FillParameter("cnt", country),
                _service.FillParameter("wgList", Config.ProActiveWgList)
            };

            return result;
        }

        private List<ProActiveDto> GetProActiveCost(DataTable table)
        {
            var proList = new List<ProActiveDto>();

            foreach (var wg in Config.ProActiveWgList.Split(','))
            {
                var pro3 = table?.Select("ProActiveModel = 3 AND Wg = '" + wg + "'").FirstOrDefault();
                var pro4 = table?.Select("ProActiveModel = 4 AND Wg = '" + wg + "'").FirstOrDefault();
                var pro6 = table?.Select("ProActiveModel = 6 AND Wg = '" + wg + "'").FirstOrDefault();
                var pro7 = table?.Select("ProActiveModel = 7 AND Wg = '" + wg + "'").FirstOrDefault();
                var oneTime = table?.Select("Wg = '" + wg + "'").FirstOrDefault();

                var proActiveCost = new ProActiveDto
                {
                    Wg = wg,
                    ProActive3 = _service.CheckDoubleField(pro3, "Cost"),
                    ProActive4 = _service.CheckDoubleField(pro4, "Cost"),
                    ProActive6 = _service.CheckDoubleField(pro6, "Cost"),
                    ProActive7 = _service.CheckDoubleField(pro7, "Cost"),
                    OneTimeTasks = _service.CheckDoubleField(oneTime, "Cost")
                };
                proList.Add(proActiveCost);
            }


            return proList;
        }      
    }
}
