using Gdc.Scd.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface IRepository<T> where T : IIdentifiable
    {
        T Get(long id);

        IQueryable<T> GetAll();

        Task<IEnumerable<T>> GetAllAsync();

        void Save(T item);

        void Save(IEnumerable<T> items);

        void Delete(long id);

        void Delete(IEnumerable<long> ids);

        void DeleteAll();

        void DisableTrigger();

        void EnableTrigger();

        bool IsNewItem<TItem>(TItem item) where TItem : class, IIdentifiable;
    }
}
