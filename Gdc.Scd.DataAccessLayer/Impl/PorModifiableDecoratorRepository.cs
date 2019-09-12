using Gdc.Scd.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class PorModifiableDecoratorRepository<T> : IRepository<T> where T : class, IIdentifiable, IModifiable, new()
    {
        private readonly EntityFrameworkRepository<T> origin;

        public PorModifiableDecoratorRepository(EntityFrameworkRepository<T> origin)
        {
            this.origin = origin;
        }

        public void Delete(long id)
        {
            var item = Get(id);
            if (item != null)
            {
                item.DeactivatedDateTime = DateTime.Now;
                Save(item);
            }
        }

        public void DeleteAll()
        {
            this.origin.DeleteAll();
        }

        public void DisableTrigger()
        {
            origin.DisableTrigger();
        }

        public void EnableTrigger()
        {
            origin.EnableTrigger();
        }

        public T Get(long id)
        {
            return origin.Get(id);
        }

        public IQueryable<T> GetAll()
        {
            return origin.GetAll();
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            return origin.GetAllAsync();
        }

        public void Save(T item)
        {
            Timestamp(item);
            origin.Save(item);
        }

        public void Save(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Timestamp(item);
            }
            origin.Save(items);
        }

        private static void Timestamp(T item)
        {
            var now = DateTime.Now;
            if (item.Id == 0)
            {
                item.CreatedDateTime = now;
            }
            item.ModifiedDateTime = now;
        }
    }
}
