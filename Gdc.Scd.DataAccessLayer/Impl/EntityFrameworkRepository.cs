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

        public T Get(Guid id)
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

        public void Delete(Guid id)
        {
            var item = new T();

            item.Id = id;

            this.SetDeleteState(item);
        }

        protected void SetAddOrUpdateState<TItem>(TItem item) where TItem : class, IIdentifiable
        {
            var entry = this.dbContext.Entry(item);

            entry.State = 
                item.Id == Guid.Empty 
                    ? EntityState.Added 
                    : EntityState.Modified;
        }

        protected void SetDeleteState<TItem>(TItem item) where TItem : class, IIdentifiable
        {
            var entry = this.dbContext.Entry(item);

            entry.State = EntityState.Deleted;
        }
    }
}
