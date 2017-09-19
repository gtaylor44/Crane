using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;

namespace SprocMapperLibrary.CacheProvider
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class AbstractCacheProvider
    {
        /// <summary>
        /// 
        /// </summary>
        internal static readonly object Padlock = new object();

        /// <summary>
        /// 
        /// </summary>
        protected SprocCachePolicy GlobalSprocPolicy;

        /// <summary>
        /// 
        /// </summary>
        protected readonly List<SprocCachePolicy> CustomSprocCachePolicyList;

        /// <summary>
        /// 
        /// </summary>
        protected AbstractCacheProvider()
        {
            CustomSprocCachePolicyList = new List<SprocCachePolicy>();
            GlobalSprocPolicy = null;
        }
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
        /// Clears all cached lists matching the specified regular expression.
        /// </summary>
        public abstract void RemoveMatchingKeys(string regex);

        /// <summary>
        /// Removes all keys from cache.
        /// </summary>
        public abstract void ResetCache();


        /// <summary>
        /// Set a custom policy on all cached items.
        /// </summary>
        /// <param name="policy">The custom policy.</param>
        public void SetGlobalPolicy(SprocCachePolicy policy)
        {
            if (PolicyIsValid(policy))
                GlobalSprocPolicy = policy;
        }

        /// <summary>
        /// Set a custom policy for a regular expression. If the regular expression matches, this policy will take precedence over the global policy (if one is set) and default policy. 
        /// </summary>
        /// <param name="regularExpression">The regular express pattern to match.</param>
        /// <param name="policy">The custom policy.</param>
        public void AddPolicy(string regularExpression, SprocCachePolicy policy)
        {
            if (policy == null)
                throw new ArgumentNullException(nameof(policy));

            if (regularExpression == null)
                throw new ArgumentNullException(nameof(regularExpression));

            policy.CacheKeyRegExp = regularExpression;

            if (PolicyIsValid(policy))
                CustomSprocCachePolicyList.Add(policy);
        }

        private bool PolicyIsValid(SprocCachePolicy policy)
        {
            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            if (policy.InfiniteExpiration && policy.AbsoluteExpiration != TimeSpan.Zero)
            {
                throw new InvalidOperationException($"Can't set expiration to infinite if {nameof(policy.AbsoluteExpiration)} is set.");
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        protected DateTimeOffset GetDateTimeOffsetFromTimespan(TimeSpan time)
        {        
            return DateTimeOffset
                .Now
                .AddDays(time.Days)
                .AddHours(time.Hours)
                .AddMinutes(time.Minutes)
                .AddSeconds(time.Seconds)
                .AddMilliseconds(time.Milliseconds);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected SprocCachePolicy GetCachingStrategy(string key)
        {
            // If specific policy exists, use it and break from loop. 
            if (CustomSprocCachePolicyList != null && CustomSprocCachePolicyList.Any())
            {
                foreach (var customPolicy in CustomSprocCachePolicyList)
                {
                    if (Regex.IsMatch(key, customPolicy.CacheKeyRegExp))
                    {
                        return customPolicy;
                    }
                }
            }

            // If no specific policies found and global policy not null, use global policy.
            if (GlobalSprocPolicy != null)
            {
                return GlobalSprocPolicy;
            }

            // If no specific policies found OR global policy set, use default policy. 
            return GetDefaultPolicy();
        }

        private SprocCachePolicy GetDefaultPolicy()
        {
            return new SprocCachePolicy()
            {
                InfiniteExpiration = true
            };
        }
    }
}
