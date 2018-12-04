using System.Linq;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IReadingDomainService<T> where T : IIdentifiable
    {
        T Get(long id);

        IQueryable<T> GetAll();
    }
}
