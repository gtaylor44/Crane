using System;

namespace SprocMapperLibrary.CacheProvider
{
    /// <summary>
    /// 
    /// </summary>
    public class SprocCachePolicy
    {
        internal string CacheKeyRegExp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public TimeSpan SlidingExpiration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan AbsoluteExpiration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Action CacheKeyAddedCallback { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Action CacheKeyRemovedCallback { get; set; }
    }
}
