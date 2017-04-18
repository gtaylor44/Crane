using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprocMapperLibrary.Base
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class AbstractCacheProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<T> Get<T>(string key);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="items"></param>
        /// <typeparam name="T"></typeparam>
        public abstract void Insert<T>(string key, IEnumerable<T> items);
    }
}
