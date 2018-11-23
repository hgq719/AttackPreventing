using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Business
{
    public class Utils
    {
        public static T GetMemoryCache<T>(string key)
        {
            MemoryCache cache = MemoryCache.Default;
            T t = (T)cache.Get(key);
            return t;
        }
        public static void SetMemoryCache<T>(string key, T value, int timeout = 30)
        {
            MemoryCache cache = MemoryCache.Default;
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow.AddMinutes(timeout));
            cache.Set(key, value, policy);
        }
        public static void RemoveMemoryCache(string key)
        {
            MemoryCache cache = MemoryCache.Default;
            cache.Remove(key);
        }
        public static T GetMemoryCache<T>(string key, Func<T> func, int timeout = 30)
        {
            T t = GetMemoryCache<T>(key);
            if (t == null)
            {
                t = func();
                SetMemoryCache<T>(key, t, timeout);
            }

            return t;
        }
    }
}
