using System;
using System.Data.SqlClient;
using Crane.Base;
using Crane.Interface;
using Crane.Shared;

namespace Crane.SqlServer
{
    /// <summary>
    /// 
    /// </summary>
    public class SqlServerAccess : BaseAccess, ICraneAccess
    {
        private const string InvalidConnMsg = "Please ensure that valid Sql Server Credentials have been passed in.";

        private readonly string _connectionString;

#if NETFRAMEWORK
        private readonly SqlCredential _credential;
#endif

        /// <inheritdoc />
        public SqlServerAccess(string connectionString, QueryOptions options = null) : base(options)
        {
            _connectionString = connectionString ?? throw new ArgumentException(InvalidConnMsg);
#if NETFRAMEWORK
            _credential = null;
#endif
        }

#if NETFRAMEWORK 
        /// <inheritdoc />
        public SqlServerAccess(string connectionString, SqlCredential credential, QueryOptions options = null) : base(options)
        {
            if (connectionString == null || credential == null)
                throw new ArgumentException(InvalidConnMsg);

            _connectionString = connectionString;
            _credential = credential;

            QueryOptions = options ?? CraneHelper.GetDefaultQueryOptions();
        }
#endif

        /// <inheritdoc />
        public BaseCommand Command()
        {
            return new SqlServerCommand(_connectionString);
        }

        /// <inheritdoc />
        public BaseQuery Query()
        {
#if NETFRAMEWORK
            return new SqlServerQuery(_connectionString, _credential, QueryOptions);
#elif NETCORE
            return new SqlServerQuery(_connectionString, QueryOptions);
#endif
        }
    }
}
