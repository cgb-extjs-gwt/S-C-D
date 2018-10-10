using System;
using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Impl;
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

        public T Get(long id)
        {
            return this.repository.Get(id);
        }

        public IQueryable<T> GetAll()
        {
            return this.repository.GetAll();
        }

        public void Save(T item)
        {
            using (var transaction = this.repositorySet.GetTransaction())
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

                    throw ex;
                }
            }
        }

        public virtual void Save(IEnumerable<T> items)
        {
            using (var transaction = this.repositorySet.GetTransaction())
            {
                try
                {
                    foreach (var item in items)
                    {
                        this.InnerSave(item);
                    }

                    this.repositorySet.Sync();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    throw ex;
                }
            }
        }

        public void Delete(long id)
        {
            using (var transaction = this.repositorySet.GetTransaction())
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

                    throw ex;
                }
            }
        }

        protected void InnerSave(T item)
        {
            this.repository.Save(item);
        }

        protected void InnerDelete(long id)
        {
            this.repository.Delete(id);
        }
    }
}
