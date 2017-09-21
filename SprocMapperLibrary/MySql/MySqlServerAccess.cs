using System;
using SprocMapperLibrary.Base;
using SprocMapperLibrary.CacheProvider;
using SprocMapperLibrary.Interface;

namespace SprocMapperLibrary.MySql
{
    /// <summary>
    /// </summary>
    public class MySqlServerAccess : BaseAccess, ISprocMapperAccess
    {
        private readonly string _connectionString;
        private const string InvalidConnMsg = "Please ensure that valid MySQL credentials have been passed in.";

        /// <inheritdoc />
        public MySqlServerAccess(string connectionString, AbstractCacheProvider cacheProvider = null)
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
