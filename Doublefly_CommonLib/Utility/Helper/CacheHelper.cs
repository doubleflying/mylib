using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace Utility.Helper
{
    public class CacheHelper
    {
        /// <summary>
        /// 获取 System.Web.Caching.Cache 对象中的指定项。
        /// </summary>
        /// <param name="key">用于引用该对象的缓存键。</param>
        /// <returns>缓存中的对象。</returns>
        public static object GetCache(string key)
        {
            var cache = HttpRuntime.Cache;
            return cache[key];
        }

        /// <summary>
        /// 获取泛型缓存对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(string key)
        {
            var cache = HttpRuntime.Cache;
            return (T)cache[key];
        }

        /// <summary>
        /// 向 System.Web.Caching.Cache 对象中插入对象。
        /// </summary>
        /// <param name="key">用于引用该对象的缓存键。</param>
        /// <param name="value">要插入缓存中的对象。</param>
        public static void SetCache(string key, object value)
        {
            var cache = HttpRuntime.Cache;
            cache.Insert(key, value);
        }

        public static void SetCache(string key, object value, int minutes)
        {
            var cache = HttpRuntime.Cache;
            cache.Insert(key, value, null, DateTime.MaxValue, new TimeSpan(0, minutes, 0), CacheItemPriority.NotRemovable, null);
        }
        /// <summary>
        /// 向 System.Web.Caching.Cache 对象中插入对象。
        /// </summary>
        /// <param name="key">用于引用该对象的缓存键。</param>
        /// <param name="value">要插入缓存中的对象。</param>
        /// <param name="expiration">
        /// 最后一次访问所插入对象时与该对象到期时之间的时间间隔。如果该值等效于 20 分钟，则对象在最后一次被访问 20 分钟之后将到期并被从缓存中移除。如果使用可调到期，则
        /// absoluteExpiration 参数必须为 System.Web.Caching.Cache.NoAbsoluteExpiration。
        /// </param>
        public static void SetCache(string key, object value, TimeSpan expiration)
        {
            var cache = HttpRuntime.Cache;
            cache.Insert(key, value, null, DateTime.MaxValue, expiration, CacheItemPriority.NotRemovable, null);
        }

        /// <summary>
        /// 向 System.Web.Caching.Cache 对象中插入对象。
        /// </summary>
        /// <param name="key">用于引用该对象的缓存键。</param>
        /// <param name="value">要插入缓存中的对象。</param>
        /// <param name="absoluteExpiration">
        /// 所插入对象将到期并被从缓存中移除的时间。要避免可能的本地时间问题（例如从标准时间改为夏时制），请使用 System.DateTime.UtcNow
        /// 而不是 System.DateTime.Now 作为此参数值。如果使用绝对到期，则 slidingExpiration 参数必须为 System.Web.Caching.Cache.NoSlidingExpiration。
        /// </param>
        /// <param name="slidingExpiration">
        /// 最后一次访问所插入对象时与该对象到期时之间的时间间隔。如果该值等效于 20 分钟，则对象在最后一次被访问 20 分钟之后将到期并被从缓存中移除。如果使用可调到期，则
        /// absoluteExpiration 参数必须为 System.Web.Caching.Cache.NoAbsoluteExpiration。
        /// </param>
        public static void SetCache(string key, object value, DateTime absoluteExpiration, TimeSpan slidingExpiration)
        {
            var cache = HttpRuntime.Cache;
            cache.Insert(key, value, null, absoluteExpiration, slidingExpiration);
        }

        /// <summary>
        /// 从应用程序的 System.Web.Caching.Cache 对象移除指定项。
        /// </summary>
        /// <param name="key">用于引用该对象的缓存键。</param>
        public static void RemoveCache(string key)
        {
            var cache = HttpRuntime.Cache;
            cache.Remove(key);
        }

        /// <summary>
        /// 移除应用程序的 System.Web.Caching.Cache 对象中所有指定项。
        /// </summary>
        public static void RemoveAllCache()
        {
            var cache = HttpRuntime.Cache;
            var cacheDic = cache.GetEnumerator();

            while (cacheDic.MoveNext())
            {
                cache.Remove(cacheDic.Key.ToString());
            }
        }
    }
}
