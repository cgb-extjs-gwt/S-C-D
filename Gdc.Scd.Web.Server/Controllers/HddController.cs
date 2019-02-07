using Gdc.Scd.BusinessLogicLayer.Dto.Calculation;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Web.Server.Impl;
using System.Threading.Tasks;
using System.Web.Http;

namespace Gdc.Scd.Web.Server.Controllers
{
    [ScdAuthorize(Permissions = new[] { PermissionConstants.Report })]
    public class HddController : ApiController
    {
        private readonly IHddRetentionService hdd;

        public HddController(IHddRetentionService hdd)
        {
            this.hdd = hdd;
        }

        [HttpGet]
        public Task<DataInfo<HddRetentionDto>> GetCost(
                [FromUri]object filter,
                [FromUri]bool approved = true,
                [FromUri]int start = 0,
                [FromUri]int limit = 50
            )
        {
            return hdd.GetCost(approved, null, start, limit).ContinueWith(x => new DataInfo<HddRetentionDto> { Items = x.Result.items, Total = x.Result.total });


            /*
            if (filter != null &&
                filter.Country > 0 &&
                IsRangeValid(start, limit) &&
                HasAccess(approved, filter.Country))
            {
                return calcSrv.GetHardwareCost(approved, filter, start, limit)
                              .ContinueWith(x => this.JsonContent(x.Result.json, x.Result.total));
            }
            else
            {
                throw this.NotFoundException();
            }*/
        }
    }
}