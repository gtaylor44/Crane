using System;
using System.Data.SqlClient;
using SprocMapperLibrary.Base;

namespace SprocMapperLibrary.SqlServer
{
    /// <summary>
    /// 
    /// </summary>
    public class SqlServerAccess : BaseAccess
    {
        private const string InvalidConnMsg = "Please ensure that valid Sql Server Credentials have been passed in.";

        private readonly string _connectionString;
        private readonly SqlCredential _credential;
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        public SqlServerAccess(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentException(InvalidConnMsg);
            _credential = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="credential"></param>
        public SqlServerAccess(string connectionString, SqlCredential credential)
        {
            if (connectionString == null || credential == null)
                throw new ArgumentException(InvalidConnMsg);

            _connectionString = connectionString;
            _credential = credential;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public SqlServerSproc Sproc()
        {
            return new SqlServerSproc(_connectionString, _credential, CacheProvider);
        }


    }
}
