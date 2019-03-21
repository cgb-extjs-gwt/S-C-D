using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System;
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

        public Task<T> Get<T>(long id) where T : class, IIdentifiable, new()
        {
            return GetItems<T>().ContinueWith(x => Array.Find(x.Result, y => y.Id == id));
        }

        public Task<T[]> GetAll<T>() where T : class, IIdentifiable, new()
        {
            return GetItems<T>();
        }

        public async Task<string[]> GetNames<T>(long[] ids) where T : NamedId, new()
        {
            var len = ids == null ? 0 : ids.Length;
            var result = new string[len];

            for (var i = 0; i < len; i++)
            {
                var item = await Get<T>(ids[i]);
                if (item != null)
                {
                    result[i] = item.Name;
                }
            }

            return result;
        }

        public async Task<string[]> GetExtNames<T>(long[] ids) where T : ExternalEntity, new()
        {
            var len = ids == null ? 0 : ids.Length;
            var result = new string[len];

            for (var i = 0; i < len; i++)
            {
                var item = await Get<T>(ids[i]);
                if (item != null)
                {
                    result[i] = item.ExternalName;
                }
            }

            return result;
        }

        public async Task<string> GetName<T>(long id) where T : NamedId, new()
        {
            var item = await Get<Country>(id);
            return item == null ? null : item.Name;
        }

        private async Task<T[]> GetItems<T>() where T : class, IIdentifiable, new()
        {
            string key = "__CacheDomainService__" + typeof(T).FullName;

            var items = MemoryCache.Default.Get(key) as T[];

            if (items == null)
            {
                //load and save to cache 1hour only...

                items = await repositorySet.GetRepository<T>().GetAll().GetAsync();

                MemoryCache.Default.Set(key, items, DateTime.Now.AddHours(1));
            }

            return items;
        }
    }
}
