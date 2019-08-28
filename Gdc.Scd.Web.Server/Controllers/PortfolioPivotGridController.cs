using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities.Pivot;
using Gdc.Scd.Core.Entities.Portfolio;
using Newtonsoft.Json;

namespace Gdc.Scd.Web.Server.Controllers
{
    public class PortfolioPivotGridController : ApiController
    {
        private readonly IPortfolioPivotGridService portfolioPivotGridService;

        public PortfolioPivotGridController(IPortfolioPivotGridService portfolioPivotGridService)
        {
            this.portfolioPivotGridService = portfolioPivotGridService;
        }

        [HttpPost]
        public async Task<PivotResult> GetData([FromBody] PortfolioPivotRequest request)
        {
            return await this.portfolioPivotGridService.GetData(request);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> PivotExcelExport()
        {
            try
            {
                var request = JsonConvert.DeserializeObject<PortfolioPivotRequest>(HttpContext.Current.Request.Form["data"]);
                var stream = await this.portfolioPivotGridService.PivotExcelExport(request);

                return this.ExcelContent(stream, "PortfolioPivot.xlsx");
            }
            catch
            {
                return this.ExcelContent(new MemoryStream(), "Error");
            }
        }
    }
}