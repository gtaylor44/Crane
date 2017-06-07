using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;

namespace SprocMapperLibrary.CacheProvider.MemoryCache
{  
    /// <summary>
    /// 
    /// </summary>
    public class MemoryCacheProvider : AbstractCacheProvider
    {
        /// <summary>
        /// 
        /// </summary>
        internal static readonly object Padlock = new object();

        private SprocCachePolicy _globalSprocPolicy;
        private readonly List<SprocCachePolicy> _customSprocCachePolicyList;

        /// <summary>
        /// 
        /// </summary>
        public MemoryCacheProvider()
        {
            _customSprocCachePolicyList = new List<SprocCachePolicy>();
            _globalSprocPolicy = null;
        }
        
        /// <summary>
        /// Returns true and initialises output param items if exists in cache, otherwise returns false and default collection. 
        /// </summary>
        public override bool TryGet<T>(string key, out IEnumerable<T> items)
        {
            lock (Padlock)
            {
                var res = (IEnumerable<T>)MemoryCacheSingleton.Instance[key];

                if (res == null)
                {
                    items = default(IEnumerable<T>);
                    return false;
                }

                lock (Padlock)
                {
                    items = res;
                    return true;
                }
            }
        }

        private void CacheEntryRemovedCallback(CacheEntryRemovedArguments arguments)
        {
            
        }

        /// <summary>
        /// Adds an IEnumerable to cache with given key. 
        /// </summary>
        public override void Add<T>(string key, IEnumerable<T> items)
        {
            CacheItemPolicy policy = null;           

            // If specific policy exists, use it and break from loop. 
            if (_customSprocCachePolicyList != null && _customSprocCachePolicyList.Any())
            {
                foreach (var customPolicy in _customSprocCachePolicyList)
                {
                    if (Regex.IsMatch(key, customPolicy.CacheKeyRegExp))
                    {
                        policy = MapCacheItemPolicy(customPolicy);
                        customPolicy.CacheKeyAddedCallback?.Invoke();
                        break;
                    }
                }
            }

            // If no specific policies found, use global policy.
            if (policy == null && _globalSprocPolicy != null)
            {
                policy = MapCacheItemPolicy(_globalSprocPolicy);
                _globalSprocPolicy.CacheKeyAddedCallback?.Invoke();
            }

            // If no specific policies found OR global policy set, use default policy. 
            if (policy == null)
            {
                policy = GetDefaultPolicy();
            }

            lock (Padlock)
            {
                MemoryCacheSingleton.Instance.Add(key, items, policy);
            }
        }

        /// <summary>
        /// Remove an item in the cache at a specified key.
        /// </summary>
        public override void Remove(string key)
        {
            lock (Padlock)
            {
                MemoryCacheSingleton.Instance.Remove(key);
            }
        }

        /// <summary>
        /// Removes a given array of keys from cache.
        /// </summary>
        /// <param name="keys"></param>
        public override void Remove(string[] keys)
        {
            foreach (var key in keys)
            {
                MemoryCacheSingleton.Instance.Remove(key);
            }
        }

        /// <summary>
        /// Removes all keys from cache.
        /// </summary>
        public override void ResetCache()
        {
            foreach (var element in MemoryCacheSingleton.Instance)
            {
                MemoryCacheSingleton.Instance.Remove(element.Key);
            }
        }

        /// <inheritdoc />
        public override void SetGlobalPolicy(SprocCachePolicy policy)
        {
            _globalSprocPolicy = policy;
        }

        /// <inheritdoc />
        public override void AddPolicy(string regularExpression, SprocCachePolicy policy)
        {
            if (policy == null)
                throw new ArgumentNullException(nameof(policy));
            policy.CacheKeyRegExp = regularExpression ?? throw new ArgumentNullException(nameof(regularExpression));

            _customSprocCachePolicyList.Add(policy);
        }

        private DateTimeOffset GetDateTimeOffsetFromTimespan(TimeSpan time)
        {
            return DateTimeOffset
                .Now
                .AddDays(time.Days)
                .AddHours(time.Hours)
                .AddMinutes(time.Minutes)
                .AddSeconds(time.Seconds)
                .AddMilliseconds(time.Milliseconds);
        }

        private CacheItemPolicy MapCacheItemPolicy(SprocCachePolicy sprocCachePolicy)
        {
            return new CacheItemPolicy
            {
                AbsoluteExpiration = GetDateTimeOffsetFromTimespan(sprocCachePolicy.AbsoluteExpiration),
                SlidingExpiration = sprocCachePolicy.SlidingExpiration,
                RemovedCallback = CacheEntryRemovedCallback
            };
        }

        private CacheItemPolicy GetDefaultPolicy()
        {
            return new CacheItemPolicy()
            {
                AbsoluteExpiration = DateTimeOffset.MaxValue
            };
        }
    }
}
