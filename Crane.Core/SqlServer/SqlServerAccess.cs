using System;
using Crane.Base;
using Crane.CacheProvider;
using Crane.Core.SqlServer;
using Crane.Interface;
using Crane.Shared;

namespace Crane.SqlServer
{
    /// <inheritdoc />
    /// <summary>
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
