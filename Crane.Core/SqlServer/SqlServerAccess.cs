using System;
using SprocMapperLibrary.Base;
using SprocMapperLibrary.CacheProvider;
using SprocMapperLibrary.Core.SqlServer;
using SprocMapperLibrary.Interface;

namespace SprocMapperLibrary.SqlServer
{
    /// <summary>
    /// 
    /// </summary>
    public class SqlServerAccess : BaseAccess, ICraneAccess
    {
        private const string InvalidConnMsg = "Please ensure that valid Sql Server Credentials have been passed in.";

        private readonly string _connectionString;

        /// <inheritdoc />
        public SqlServerAccess(string connectionString, AbstractCraneCacheProvider cacheProvider = null)
        {
            _connectionString = connectionString ?? throw new ArgumentException(InvalidConnMsg);
            CacheProvider = cacheProvider;
        }

        /// <inheritdoc />
        public BaseCommand Command()
        {
            return new SqlServerCommand(_connectionString);
        }

        /// <inheritdoc />
        public BaseQuery Query()
        {
            return new SqlServerQuery(_connectionString, CacheProvider);
        }
    }
}
