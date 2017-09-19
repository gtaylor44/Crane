using System;
using SprocMapperLibrary.Base;
using SprocMapperLibrary.CacheProvider;
using SprocMapperLibrary.Interface;

namespace SprocMapperLibrary.MySql
{
    /// <summary>
    /// 
    /// </summary>
    public class MySqlServerAccess : BaseAccess, ISprocMapperAccess
    {
        private readonly string _connectionString;
        private const string InvalidConnMsg = "Please ensure that valid MySQL credentials have been passed in.";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="cacheProvider"></param>
        public MySqlServerAccess(string connectionString, AbstractCacheProvider cacheProvider = null)
        {
            if (connectionString == null)
                throw new ArgumentException(InvalidConnMsg);
            _connectionString = connectionString;
            CacheProvider = cacheProvider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public BaseSproc Sproc()
        {
            return new MySqlSproc(_connectionString, CacheProvider);
        }

        public BaseCommand Command()
        {
            throw new NotImplementedException();
        }

        public BaseQuery Query()
        {
            throw new NotImplementedException();
        }
    }
}
