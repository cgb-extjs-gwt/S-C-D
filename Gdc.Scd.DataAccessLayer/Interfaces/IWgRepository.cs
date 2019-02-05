using System.Linq;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface IWgRepository : IRepository<Wg>
    {
        IQueryable<Wg> GetStandards();
    }
}
