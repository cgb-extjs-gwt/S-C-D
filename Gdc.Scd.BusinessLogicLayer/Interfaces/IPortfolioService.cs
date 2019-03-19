using Gdc.Scd.BusinessLogicLayer.Dto.Portfolio;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IPortfolioService
    {
        Task Allow(PortfolioRuleSetDto set);

        Task Deny(PortfolioRuleSetDto set);

        Task Deny(long[] countryId, long[] ids);

        Task<(PortfolioDto[] items, int total)> GetAllowed(PortfolioFilterDto filter, int start, int limit);

        Task<(PortfolioHistoryDto[] items, int total)> GetHistory(int start, int limit);
    }
}
