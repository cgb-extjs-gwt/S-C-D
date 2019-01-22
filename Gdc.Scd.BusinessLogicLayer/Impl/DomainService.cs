using System;
using System.Collections.Generic;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class DomainService<T> : ReadingDomainService<T>, IDomainService<T> where T : class, IIdentifiable, new()
    {
        public DomainService(IRepositorySet repositorySet) 
            : base(repositorySet)
        {
        }

        public virtual void Save(T item)
        {
            using (var transaction = this.repositorySet.GetTransaction())
            {
                try
                {
                    this.SaveWithoutTransaction(item);

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
                    this.SaveWithoutTransaction(items);

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    throw ex;
                }
            }
        }

        public void SaveWithoutTransaction(T item)
        {
            this.InnerSave(item);
            this.repositorySet.Sync();
        }

        public void SaveWithoutTransaction(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                this.InnerSave(item);
            }

            this.repositorySet.Sync();
        }

        public virtual void Delete(long id)
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

        protected virtual void InnerSave(T item)
        {
            this.repository.Save(item);
        }

        protected virtual void InnerDelete(long id)
        {
            this.repository.Delete(id);
        }
    }
}
