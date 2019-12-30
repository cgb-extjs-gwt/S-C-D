using Gdc.Scd.Core.Entities;
using System.Linq;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IWgService : IDomainService<Wg>
    {
        IQueryable<Wg> GetStandards();

        IQueryable<Wg> GetHardware();

        IQueryable<Wg> GetNotNotified();
    }
}
