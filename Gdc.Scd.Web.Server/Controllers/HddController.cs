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

        [HttpPost]
        public Task<DataInfo<HddRetentionDto>> GetCost([FromBody]HddFilterDto filter)
        {
            if (IsRangeValid(filter.Start, filter.Limit))
            {
                return hdd.GetCost(this.CurrentUser(), filter.Approved, filter, filter.Start, filter.Limit)
                          .ContinueWith(x =>
                          {
                              var total = (filter.Page - 1) * filter.Limit + x.Result.items.Length;
                              if (x.Result.hasMore)
                              {
                                  total++;
                              }
                              return new DataInfo<HddRetentionDto> { Items = x.Result.items, Total = total };
                          });
            }
            else
            {
                throw this.NotFoundException();
            }
        }

        [HttpPost]
        public IHttpActionResult SaveCost([FromBody]HddRetentionDto[] items)
        {
            var usr = this.CurrentUser();
            if (hdd.CanEdit(usr))
            {
                hdd.SaveCost(usr, items);
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        private bool IsRangeValid(int start, int limit)
        {
            return start >= 0 && limit <= 100;
        }
    }
}