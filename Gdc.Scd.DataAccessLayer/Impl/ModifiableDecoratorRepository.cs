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
        protected readonly EntityFrameworkRepository<T> origin;

        public ModifiableDecoratorRepository(EntityFrameworkRepository<T> origin)
        {
            this.origin = origin;
        }

        public void Delete(long id)
        {
            this.Delete(new[] { id });
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

        public virtual T Get(long id)
        {
            return GetAll().Where(x => x.Id == id).FirstOrDefault();
        }

        public virtual IQueryable<T> GetAll()
        {
            return origin.GetAll().Where(x => !x.DeactivatedDateTime.HasValue);
        }

        public virtual Task<IEnumerable<T>> GetAllAsync()
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
            var now = DateTime.UtcNow;
            if (item.Id == 0)
            {
                item.CreatedDateTime = now;
            }
            item.ModifiedDateTime = now;
        }

        public void Delete(IEnumerable<long> ids)
        {
            var deactivatedDateTime = DateTime.UtcNow;
            var items = this.GetAll().Where(item => ids.Contains(item.Id)).ToArray();

            foreach (var item in items)
            {
                item.DeactivatedDateTime = deactivatedDateTime;
            }

            this.Save(items);
        }

        public bool IsNewItem<TItem>(TItem item) where TItem : class, IIdentifiable
        {
            return this.origin.IsNewItem(item);
        }
    }
}
