using Gdc.Scd.BusinessLogicLayer.Dto.Portfolio;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Web.Server.Impl;
using System.Threading.Tasks;
using System.Web.Http;

namespace Gdc.Scd.Web.Server.Controllers
{
    [ScdAuthorize(Permissions = new[] { PermissionConstants.Portfolio })]
    public class PortfolioController : ApiController
    {
        private readonly IPortfolioService portfolioService;

        public PortfolioController(IPortfolioService portfolioService)
        {
            this.portfolioService = portfolioService;
        }

        [HttpGet]
        public Task<DataInfo<PortfolioDto>> Allowed(
                [FromUri]PortfolioFilterDto filter,
                [FromUri]int start = 0,
                [FromUri]int limit = 25
            )
        {
            if (!IsRangeValid(start, limit))
            {
                return null;
            }

            return portfolioService.GetAllowed(filter, start, limit)
                                   .ContinueWith(x => new DataInfo<PortfolioDto>
                                   {
                                       Items = x.Result.Item1,
                                       Total = x.Result.Item2
                                   });
        }

        [HttpPost]
        public async Task<object> Allow([FromBody]PortfolioRuleSetDto m)
        {
            await portfolioService.Allow(m); //wait result, or error
            return OkResult();
        }

        [HttpPost]
        public async Task<object> Deny([FromBody]PortfolioRuleSetDto m)
        {
            await portfolioService.Deny(m); //wait result, or error
            return OkResult();
        }

        private bool IsRangeValid(int start, int limit)
        {
            return start >= 0 && limit <= 50;
        }

        object OkResult()
        {
            return new { ok = true };
        }
    }
}
