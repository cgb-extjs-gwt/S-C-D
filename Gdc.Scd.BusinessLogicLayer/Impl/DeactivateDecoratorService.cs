using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class DeactivateDecoratorService<T> : IDomainService<T> where T : class, IIdentifiable, IDeactivatable, new()
    {
        private readonly DomainService<T> origin;

        public DeactivateDecoratorService(DomainService<T> origin)
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

        public T Get(long id)
        {
            return GetAll().Where(x => x.Id == id).FirstOrDefault();
        }

        public IQueryable<T> GetAll()
        {
            return origin.GetAll().Where(x => !x.DeactivatedDateTime.HasValue);
        }

        public void Save(T item)
        {
            origin.Save(item);
        }

        public void Save(IEnumerable<T> items)
        {
            origin.Save(items);
        }

        public void SaveWithoutTransaction(T item)
        {
            origin.SaveWithoutTransaction(item);
        }

        public void SaveWithoutTransaction(IEnumerable<T> items)
        {
            origin.SaveWithoutTransaction(items);
        }
    }
}
