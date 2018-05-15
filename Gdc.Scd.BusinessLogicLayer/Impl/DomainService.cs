using System;
using System.Linq;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class DomainService<T> : IDomainService<T> where T : class, IIdentifiable, new()
    {
        protected readonly IRepositorySet repositorySet;
        protected readonly IRepository<T> repository;

        public DomainService(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
            this.repository = this.repositorySet.GetRepository<T>();
        }

        public T Get(Guid id)
        {
            return this.repository.Get(id);
        }

        public IQueryable<T> GetAll()
        {
            return this.repository.GetAll();
        }

        public void Save(T item)
        {
            using (var transaction = this.repositorySet.BeginTransaction())
            {
                try
                {

                    this.InnerSave(item);
                    this.repositorySet.Sync();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }
            }
        }

        public void Delete(Guid id)
        {
            using (var transaction = this.repositorySet.BeginTransaction())
            {
                try
                {

                    this.InnerDelete(id);
                    this.repositorySet.Sync();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }
            }
        }

        protected void InnerSave(T item)
        {
            this.repository.Save(item);
        }

        protected void InnerDelete(Guid id)
        {
            this.repository.Delete(id);
        }
    }
}
