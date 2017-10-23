using System.Collections.Generic;

namespace Crane.Shared.Interface
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICraneCacheProvider
    {
        /// <summary>
        /// When this method returns, contains the items associated with the specified key, 
        /// if the key is found; otherwise, the default value for the type of the value parameter. 
        /// </summary>
        /// <returns></returns>
        bool TryGet<T>(string key, out IEnumerable<T> items);

        /// <summary>
        /// Adds a list of cacheable records.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="items"></param>
        /// <typeparam name="T"></typeparam>
        void Add<T>(string key, IEnumerable<T> items);

        /// <summary>
        /// Clears a single cached list at the specified key.
        /// </summary>
        void Remove(string key);

        /// <summary>
        /// Removes all keys from cache.
        /// </summary>
        void ResetCache();
    }
}
