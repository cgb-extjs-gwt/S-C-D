using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities.Portfolio;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface IPortfolioRepository<TPortfolio, TInheritance> : IRepository<TPortfolio>
        where TPortfolio : Portfolio, new()
        where TInheritance : BasePortfolioInheritance, new()
    {
        Task<IEnumerable<TInheritance>> GetInheritanceItems(long[] plaIds);
    }
}
