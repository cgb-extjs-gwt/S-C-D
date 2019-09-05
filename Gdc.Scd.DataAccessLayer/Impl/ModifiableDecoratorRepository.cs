using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class ModifiableDecoratorRepository<T> : IRepository<T> where T : class, IIdentifiable, IModifiable, new()
    {
        private readonly EntityFrameworkRepository<T> origin;

        public ModifiableDecoratorRepository(EntityFrameworkRepository<T> origin)
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
            throw new NotSupportedException();
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
            return GetAll().Where(x => x.Id == id).FirstOrDefault();
        }

        public IQueryable<T> GetAll()
        {
            return origin.GetAll().Where(x => !x.DeactivatedDateTime.HasValue);
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            return GetAll().GetAsync().ContinueWith(x => (IEnumerable<T>)x.Result);
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
