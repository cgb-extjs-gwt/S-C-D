using Gdc.Scd.BusinessLogicLayer.Dto.Portfolio;
using System;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IPortfolioService
    {
        Task Allow(PortfolioRuleSetDto set);

        Task Deny(PortfolioRuleSetDto set);

        Task<Tuple<PortfolioDto[], int>> GetAllowed(PortfolioFilterDto filter, int start, int limit);

        Task<Tuple<PortfolioDto[], int>> GetDenied(PortfolioFilterDto filter, int start, int limit);
    }
}
