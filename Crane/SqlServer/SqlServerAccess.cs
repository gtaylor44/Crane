using System;
using System.Data.SqlClient;
using SprocMapperLibrary.Base;
using SprocMapperLibrary.CacheProvider;
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
        private readonly SqlCredential _credential;


        /// <inheritdoc />
        public SqlServerAccess(string connectionString, AbstractCraneCacheProvider cacheProvider = null)
        {
            if (connectionString == null)
                throw new ArgumentException(InvalidConnMsg);

            _connectionString = connectionString;
            _credential = null;
            CacheProvider = cacheProvider;
        }

        /// <inheritdoc />
        public SqlServerAccess(string connectionString, SqlCredential credential, AbstractCraneCacheProvider cacheProvider = null)
        {
            if (connectionString == null || credential == null)
                throw new ArgumentException(InvalidConnMsg);

            _connectionString = connectionString;
            _credential = credential;
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
            return new SqlServerQuery(_connectionString, _credential, CacheProvider);
        }
    }
}
