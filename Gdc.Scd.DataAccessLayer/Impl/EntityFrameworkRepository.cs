using System;
using System.Linq;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class EntityFrameworkRepository<T> : IRepository<T> where T : class, IIdentifiable, new()
    {
        protected DbContext dbContext;

        public void SetDbContext(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public T Get(long id)
        {
            return this.GetAll().FirstOrDefault(item => item.Id == id);
        }

        public IQueryable<T> GetAll()
        {
            return this.dbContext.Set<T>();
        }

        public void Save(T item)
        {
            this.SetAddOrUpdateState(item);
        }

        public void Delete(long id)
        {
            var item = new T();

            item.Id = id;

            this.SetDeleteState(item);
        }

        protected void SetAddOrUpdateState<TItem>(TItem item) where TItem : class, IIdentifiable
        {
            var entry = this.dbContext.Entry(item);

            if (item.Id == 0)
            {
                entry.State = EntityState.Added;
            }
            else
            {
                entry.State = EntityState.Modified;
            }
        }

        protected void SetDeleteState<TItem>(TItem item) where TItem : class, IIdentifiable
        {
            var entry = this.dbContext.Entry(item);

            entry.State = EntityState.Deleted;
        }
    }
}
