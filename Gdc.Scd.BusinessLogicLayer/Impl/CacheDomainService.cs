using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CacheDomainService
    {
        private readonly IRepositorySet repositorySet;

        public CacheDomainService(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public T Get<T>(long id) where T : class, IIdentifiable, new()
        {
            return Array.Find(GetItems<T>(), y => y.Id == id);
        }

        public string[] GetNames<T>(long[] ids) where T : NamedId, new()
        {
            var len = ids == null ? 0 : ids.Length;
            var result = new string[len];

            for (var i = 0; i < len; i++)
            {
                var item = this.Get<T>(ids[i]);
                if (item != null)
                {
                    result[i] = item.Name;
                }
            }

            return result;
        }

        public string[] GetExtNames<T>(long[] ids) where T : ExternalEntity, new()
        {
            var len = ids == null ? 0 : ids.Length;
            var result = new string[len];

            for (var i = 0; i < len; i++)
            {
                var item = this.Get<T>(ids[i]);
                if (item != null)
                {
                    result[i] = item.ExternalName;
                }
            }

            return result;
        }

        public string GetName<T>(long id) where T : NamedId, new()
        {
            var item = this.Get<Country>(id);
            return item == null ? null : item.Name;
        }

        private T[] GetItems<T>() where T : class, IIdentifiable, new()
        {
            string key = "__CacheDomainService__" + typeof(T).FullName;

            var items = MemoryCache.Default.Get(key) as T[];

            if (items == null)
            {
                //load and save to cache 1hour only...

                items = repositorySet.GetRepository<T>().GetAll().ToArray();

                MemoryCache.Default.Set(key, items, DateTime.Now.AddHours(1));
            }

            return items;
        }
    }
}
