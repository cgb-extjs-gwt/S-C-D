using System.Threading.Tasks;
using System.Web.Http;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities.Pivot;

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
        public async Task<PivotResult> GetData([FromBody] PivotRequest request)
        {
            return await this.portfolioPivotGridService.GetData(request);
        }
    }
}