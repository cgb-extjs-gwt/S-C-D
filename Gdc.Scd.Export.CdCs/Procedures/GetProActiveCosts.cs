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
                    ProActive3 = pro3 != null ? pro3.Field<double>("Cost") : 0,
                    ProActive4 = pro4 != null ? pro4.Field<double>("Cost") : 0,
                    ProActive6 = pro6 != null ? pro6.Field<double>("Cost") : 0,
                    ProActive7 = pro7 != null ? pro7.Field<double>("Cost") : 0,
                    OneTimeTasks = oneTime != null ? oneTime.Field<double>("OneTimeTasks") : 0
                };
                proList.Add(proActiveCost);
            }


            return proList;
        }      
    }
}
