using System;
using System.Runtime.Caching;

namespace Gdc.Scd.Web.Server
{
    public class AppCache
    {
        public static void Set(string key, object v)
        {
            Set(key, v, DateTime.Now.AddMinutes(10));
        }

        public static void Set(string key, object v, DateTime expired)
        {
            MemoryCache.Default.Add(key, v, expired);
        }

        public static object Get(string key)
        {
            return MemoryCache.Default.Get(key);
        }
    }
}