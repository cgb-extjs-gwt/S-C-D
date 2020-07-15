using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Web.Server.Impl;
using System.Web.Http;

namespace Gdc.Scd.Web.Api.Controllers
{
    [ScdAuthorize(Permissions = new[] { PermissionConstants.Report })]
    public class CalcDetailController : ApiController
    {
        private readonly CalcDetailService detailService;

        public CalcDetailController(CalcDetailService detailService)
        {
            this.detailService = detailService;
        }

        [HttpGet]
        public object Hw(long id, bool approved, string what)
        {
            return detailService.GetHwCostDetails(approved, id, what);
        }

        [HttpGet]
        public object Sw(long id, bool approved, string what)
        {
            return detailService.GetSwCostDetails(approved, id, what);
        }

        [HttpGet]
        public object SwProactive(long id, string fsp, bool approved)
        {
            return detailService.GetSwProactiveCostDetails(approved, id, fsp);
        }
    }
}
