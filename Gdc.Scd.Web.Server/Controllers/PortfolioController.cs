using Gdc.Scd.BusinessLogicLayer.Dto.Portfolio;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.Portfolio;
using Gdc.Scd.Web.Server.Impl;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Gdc.Scd.Web.Server.Controllers
{
    [ScdAuthorize(Permissions = new[] { PermissionConstants.Portfolio })]
    public class PortfolioController : ApiController
    {
        private readonly IPortfolioService portfolioService;

        private readonly IWgService wgService;

        public PortfolioController(IPortfolioService portfolioService, IWgService wgService)
        {
            this.portfolioService = portfolioService;
            this.wgService = wgService;
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
        public void Allow([FromBody]PortfolioRuleSetDto m)
        {
            portfolioService.Allow(m);
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
                    .GetHistory(filter.Start, filter.Limit)
                    .ContinueWith(x => new DataInfo<PortfolioHistoryDto> { Items = x.Result.items, Total = x.Result.total });
        }

        [HttpGet]
        public IEnumerable<NamedId> GetNotNotified()
        {
            return
                this.wgService.GetNotNotified()
                              .Select(wg => new NamedId
                              {
                                  Id = wg.Id,
                                  Name = wg.Name
                              });
        }

        [HttpPost]
        public void NotifyCountryUsers(NamedId[] wgDtos)
        {
            var wgIds = wgDtos.Select(wg => wg.Id).ToArray();

            this.portfolioService.NotifyCountryUsers(wgIds);
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
        public int Start { get; set; }

        public int Limit { get; set; }
    }
}
