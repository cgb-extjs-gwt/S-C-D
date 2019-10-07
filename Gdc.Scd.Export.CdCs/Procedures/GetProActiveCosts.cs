using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using Gdc.Scd.Export.CdCs.Dto;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Gdc.Scd.Export.CdCs.Procedures
{
    public class GetProActiveCosts
    {
        private const string GET_PROACTIVE_BY_COUNTRY_AND_WG = "Report.GetProActiveByCountryAndWg";

        private readonly CommonService _service;

        public GetProActiveCosts(CommonService service)
        {
            _service = service;
        }

        public List<ProActiveDto> Execute(string country)
        {
            var data = _service.ExecuteAsTable(GET_PROACTIVE_BY_COUNTRY_AND_WG, FillParameters(country));
            return GetProActiveCost(data);
        }

        private DbParameter[] FillParameters(string country)
        {
            return new DbParameter[] {
                new DbParameterBuilder().WithName("cnt").WithValue(country).Build(),
                new DbParameterBuilder().WithName("wgList").WithValue(Config.ProActiveWgList).Build(),
            };
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
                    ProActive3 = CommonService.CheckDoubleField(pro3, "Cost"),
                    ProActive4 = CommonService.CheckDoubleField(pro4, "Cost"),
                    ProActive6 = CommonService.CheckDoubleField(pro6, "Cost"),
                    ProActive7 = CommonService.CheckDoubleField(pro7, "Cost"),
                    OneTimeTasks = CommonService.CheckDoubleField(oneTime, "OneTimeTasks")
                };
                proList.Add(proActiveCost);
            }

            return proList;
        }
    }
}
