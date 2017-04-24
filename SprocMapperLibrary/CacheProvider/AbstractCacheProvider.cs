using System.Collections.Generic;

namespace SprocMapperLibrary.CacheProvider
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class AbstractCacheProvider
    {
        /// <summary>
        /// When this method returns, contains the items associated with the specified key, 
        /// if the key is found; otherwise, the default value for the type of the value parameter. 
        /// This parameter is passed uninitialized.
        /// </summary>
        /// <returns></returns>
        public abstract bool TryGet<T>(string key, out IEnumerable<T> items);

        /// <summary>
        /// Adds a list of cacheable records.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="items"></param>
        /// <typeparam name="T"></typeparam>
        public abstract void Add<T>(string key, IEnumerable<T> items);

        /// <summary>
        /// Clears a single cached list at the specified key.
        /// </summary>
        public abstract void Remove(string key);

        /// <summary>
        /// Removes a given array of keys from cache.
        /// </summary>
        /// <param name="keys"></param>
        public abstract void Remove(string[] keys);

        /// <summary>
        /// Removes all keys from cache.
        /// </summary>
        public abstract void ResetCache();
    }
}
