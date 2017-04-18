using System;
using MySql.Data.MySqlClient;
using SprocMapperLibrary.Base;

namespace SprocMapperLibrary.MySql
{
    /// <summary>
    /// 
    /// </summary>
    public class MySqlServerAccess
    {
        private AbstractCacheProvider _cacheProvider;
        private readonly MySqlConnection _conn;
        private const string InvalidConnMsg = "Please ensure that valid MySQL credentials have been passed in.";
        private const string InvalidCacheMsg = "Cache provider already registered.";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        public MySqlServerAccess(string connectionString)
        {
            if (connectionString == null)
                throw new ArgumentException(InvalidConnMsg);

            _conn = new MySqlConnection(connectionString);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public MySqlSproc Sproc()
        {
            return new MySqlSproc(_conn, _cacheProvider);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cacheProvider"></param>
        public void RegisterCacheProvider(AbstractCacheProvider cacheProvider)
        {
            if (_cacheProvider != null)
                throw new InvalidOperationException(InvalidCacheMsg);

            _cacheProvider = cacheProvider;
        }
    }
}
