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

        [HttpPost]
        public Task<DataInfo<PortfolioDto>> Allowed([FromBody]PortfolioFilterDto filter)
        {
            if (!IsRangeValid(filter.Start, filter.Limit))
            {
                return null;
            }

            return portfolioService
                    .GetAllowed(filter, filter.Start, filter.Limit)
                    .ContinueWith(x => new DataInfo<PortfolioDto> { Items = x.Result.items, Total = x.Result.total });
        }

        [HttpPost]
        public Task Allow([FromBody]PortfolioRuleSetDto m)
        {
            return portfolioService.Allow(m);
        }

        [HttpPost]
        public Task Deny([FromBody]PortfolioRuleSetDto m)
        {
            return portfolioService.Deny(m);
        }

        [HttpPost]
        public Task DenyLocal([FromBody]LocalPortfolioDto m)
        {
            return portfolioService.Deny(m.CountryId, m.Items);
        }

        [HttpPost]
        public Task<DataInfo<PortfolioHistoryDto>> History([FromBody]PortfolioHistoryFilterDto filter)
        {
            if (!IsRangeValid(filter.Start, filter.Limit))
            {
                return null;
            }

            return portfolioService
                    .GetHistory(filter.Country, filter.Start, filter.Limit)
                    .ContinueWith(x => new DataInfo<PortfolioHistoryDto> { Items = x.Result.items, Total = x.Result.total });
        }

        private bool IsRangeValid(int start, int limit)
        {
            return start >= 0 && limit <= 100;
        }
    }

    public class LocalPortfolioDto
    {
        public long[] CountryId { get; set; }

        public long[] Items { get; set; }
    }

    public class PortfolioHistoryFilterDto
    {
        public long? Country { get; set; }

        public int Start { get; set; }

        public int Limit { get; set; }
    }
}
