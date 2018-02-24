using System;
using Crane.Base;
using Crane.CacheProvider;
using Crane.Interface;
using Crane.Shared;

namespace Crane.MySql
{
    /// <summary>
    /// </summary>
    public class MySqlServerAccess : BaseAccess, ICraneAccess
    {
        private readonly string _connectionString;
        private const string InvalidConnMsg = "Please ensure that valid MySQL credentials have been passed in.";

        /// <inheritdoc />
        public MySqlServerAccess(string connectionString, AbstractCraneCacheProvider cacheProvider = null)
        {
            if (connectionString == null)
                throw new ArgumentException(InvalidConnMsg);

            _connectionString = connectionString;
            CacheProvider = cacheProvider;
        }

        /// <inheritdoc />
        public BaseCommand Command()
        {
            return new MySqlUserCommand(_connectionString);
        }

        /// <inheritdoc />
        public BaseQuery Query()
        {
            return new MySqlUserQuery(_connectionString, CacheProvider);
        }
    }
}
