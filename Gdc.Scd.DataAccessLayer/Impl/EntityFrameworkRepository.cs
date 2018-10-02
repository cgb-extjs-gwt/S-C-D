using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gdc.Scd.DataAccessLayer.Impl
{
    public class EntityFrameworkRepository<T> : IRepository<T> where T : class, IIdentifiable, new()
    {
        protected readonly EntityFrameworkRepositorySet repositorySet;

        public EntityFrameworkRepository(EntityFrameworkRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public virtual T Get(long id)
        {
            return this.GetAll().FirstOrDefault(item => item.Id == id);
        }

        public virtual IQueryable<T> GetAll()
        {
            return this.repositorySet.Set<T>();
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            return this.repositorySet.Set<T>()
                                     .ToArrayAsync()
                                     .ContinueWith(x => (IEnumerable<T>)x.Result);
        }

        public virtual void Save(T item)
        {
            this.AddOrUpdate(item);
        }

        public virtual void Save(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                this.Save(item);
            }
        }

        public virtual void Delete(long id)
        {
            var item = new T
            {
                Id = id
            };

            this.SetDeleteState(item);
        }

        protected void SetAddOrUpdateState<TItem>(TItem item) where TItem : class, IIdentifiable
        {
            if (item != null)
            {
                var entry = this.repositorySet.Entry(item);

                if (entry.State == EntityState.Detached)
                {
                    if (this.IsNewItem(item))
                    {
                        entry.State = EntityState.Added;
                    }
                    else
                    {
                        entry.State = EntityState.Modified;
                    }
                }
            }
        }

        protected void SetAddOrUpdateStateCollection<TItem>(IEnumerable<TItem> items) where TItem : class, IIdentifiable
        {
            foreach (var item in items)
            {
                this.SetAddOrUpdateState(item);
            }
        }

        protected void SetDeleteState<TItem>(TItem item) where TItem : class, IIdentifiable
        {
            if (item != null)
            {
                if (!this.IsNewItem(item))
                {
                    var entry = this.repositorySet.Entry(item);
                    entry.State = EntityState.Deleted;
                }                  
            }
        }

        protected bool IsNewItem<TItem>(TItem item) where TItem : class, IIdentifiable
        {
            return item.Id == default(long);
        }

        protected void AddOrUpdate<TItem>(TItem item) where TItem : class, IIdentifiable
        {
            if (item != null)
            {
                var set = this.repositorySet.Set<TItem>();

                if (this.IsNewItem(item))
                {
                    set.Add(item);
                }
                else
                {
                    set.Update(item);
                }
            }

            
        }

        public virtual void DeleteAll()
        {
            var tableName = GetTableName();
            this.repositorySet.ExecuteSql($"TRUNCATE TABLE {tableName}");
        }

        private string GetTableName()
        {
            var mapping = this.repositorySet.Model.FindEntityType(typeof(T)).Relational();
            var schema = mapping.Schema;
            var tableName = mapping.TableName;
            return $"[{schema}].[{tableName}]";
        }
    }
}
