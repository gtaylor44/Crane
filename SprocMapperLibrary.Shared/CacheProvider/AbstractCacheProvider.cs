using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SprocMapperLibrary.Shared.Interface;

namespace SprocMapperLibrary.CacheProvider
{
    /// <inheritdoc />
    public abstract class AbstractCacheProvider : ICacheProvider
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

        /// <inheritdoc />
        public abstract bool TryGet<T>(string key, out IEnumerable<T> items);

        /// <inheritdoc />
        public abstract void Add<T>(string key, IEnumerable<T> items);

        /// <inheritdoc />
        public abstract void Remove(string key);

        /// <inheritdoc />
        public abstract void ResetCache();

        /// <summary>
        /// Set a custom policy on all cached items.
        /// </summary>
        /// <param name="policy">The custom policy.</param>
        public void AddGlobalPolicy(SprocCachePolicy policy)
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
            policy.CacheKeyRegExp = regularExpression ?? throw new ArgumentNullException(nameof(regularExpression));

            if (PolicyIsValid(policy))
                CustomSprocCachePolicyList.Add(policy);
        }

        private bool PolicyIsValid(SprocCachePolicy policy)
        {
            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            if (policy.AbsoluteExpiration != TimeSpan.Zero && policy.SlidingExpiration != TimeSpan.Zero)
            {
                throw new InvalidOperationException($"Cache Policy is invalid. AbsoluteExpiration and SlidingExpiration can't both be set. To resolve this issue, set one or the other.");
            }

            if (policy.SlidingExpiration != TimeSpan.Zero && policy.InfiniteExpiration)
            {
                throw new InvalidOperationException($"Cache Policy is invalid. SlidingExpiration can't be set if InfiniteExpiration is set to true. Set InfiniteExpiration to false if you want to use SlidingExpiration.");
            }

            if (policy.InfiniteExpiration && policy.AbsoluteExpiration != TimeSpan.Zero)
            {
                throw new InvalidOperationException($"Cache Policy is invalid. Can't set expiration to infinite if {nameof(policy.AbsoluteExpiration)} is set.");
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

        private static SprocCachePolicy GetDefaultPolicy()
        {
            return new SprocCachePolicy()
            {
                InfiniteExpiration = true
            };
        }
    }
}
