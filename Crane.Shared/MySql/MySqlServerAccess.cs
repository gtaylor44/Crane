using System;
using Crane.Base;
using Crane.CacheProvider;
using Crane.Interface;
using Crane.Shared;
using Crane.Shared.Base;

namespace Crane.MySql
{
    /// <summary>
    /// </summary>
    public class MySqlServerAccess : BaseAccess, ICraneAccess
    {
        private readonly string _connectionString;
        private const string InvalidConnMsg = "Please ensure that valid MySQL credentials have been passed in.";

        /// <inheritdoc />
        public MySqlServerAccess(string connectionString, QueryOptions options = null) : base(options)
        {
            if (connectionString == null)
                throw new ArgumentException(InvalidConnMsg);

            _connectionString = connectionString;
        }

        /// <inheritdoc />
        public BaseCommand Command()
        {
            return new MySqlUserCommand(_connectionString);
        }

        /// <inheritdoc />
        public BaseQuery Query()
        {
            return new MySqlUserQuery(_connectionString, QueryOptions);
        }
    }
}
