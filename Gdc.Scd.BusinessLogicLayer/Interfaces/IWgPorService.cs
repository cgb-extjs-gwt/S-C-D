using Gdc.Scd.Core.Entities;
using System.Linq;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IWgPorService : IDomainService<Wg>
    {
        IQueryable<Wg> GetStandards();
    }
}
