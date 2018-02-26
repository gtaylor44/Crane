using Crane.CacheProvider;
using System;
using System.Collections.Generic;
using System.Text;

namespace Crane
{
    /// <summary>
    /// 
    /// </summary>
    public class QueryOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public AbstractCraneCacheProvider CacheProvider { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool ValidateSelectColumns { get; set; } = false;
    }
}
