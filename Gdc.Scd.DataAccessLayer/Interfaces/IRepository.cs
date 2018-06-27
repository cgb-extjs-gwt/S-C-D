using System.Linq;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface IRepository<T> where T : IIdentifiable
    {
        T Get(long id);

        IQueryable<T> GetAll();

        void Save(T item);

        void Delete(long id);
    }
}
