using System;
using SprocMapperLibrary.Base;
using SprocMapperLibrary.CacheProvider;
using SprocMapperLibrary.Interface;

namespace SprocMapperLibrary.SqlServer
{
    /// <summary>
    /// 
    /// </summary>
    public class SqlServerAccess : BaseAccess, ISprocMapperAccess
    {
        private const string InvalidConnMsg = "Please ensure that valid Sql Server Credentials have been passed in.";

        private readonly string _connectionString;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="cacheProvider"></param>
        public SqlServerAccess(string connectionString, AbstractCacheProvider cacheProvider = null)
        {
            _connectionString = connectionString ?? throw new ArgumentException(InvalidConnMsg);
            CacheProvider = cacheProvider;
        }

        public BaseCommand Command()
        {
            return new SqlServerCommand(_connectionString);
        }

        public BaseQuery Query()
        {
            return new SqlServerQuery(_connectionString, CacheProvider);
        }
    }
}
