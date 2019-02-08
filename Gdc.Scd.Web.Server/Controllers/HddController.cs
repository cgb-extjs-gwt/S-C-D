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
                [FromUri]HddFilterDto filter,
                [FromUri]bool approved = true,
                [FromUri]int start = 0,
                [FromUri]int limit = 50
            )
        {
            if (IsRangeValid(start, limit))
            {
                return hdd.GetCost(this.CurrentUser(), approved, filter, start, limit)
                          .ContinueWith(x => new DataInfo<HddRetentionDto> { Items = x.Result.items, Total = x.Result.total });
            }
            else
            {
                throw this.NotFoundException();
            }
        }

        [HttpPost]
        public void SaveCost([FromBody]HddRetentionDto[] items)
        {
            if (true)
            {
                hdd.SaveCost(this.CurrentUser(), items);
            }
            else
            {
                throw this.NotFoundException();
            }
        }

        private bool IsRangeValid(int start, int limit)
        {
            return start >= 0 && limit <= 100;
        }
    }
}