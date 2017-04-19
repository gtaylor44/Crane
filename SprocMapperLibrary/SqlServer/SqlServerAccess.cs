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
        private readonly SqlConnection _conn;     
        private const string InvalidConnMsg = "Please ensure that valid Sql Server Credentials have been passed in.";
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        public SqlServerAccess(string connectionString)
        {
            if (connectionString == null)
                throw new ArgumentException(InvalidConnMsg);

            _conn = new SqlConnection(connectionString);
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

            _conn = new SqlConnection(connectionString, credential);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public SqlServerSproc Sproc()
        {
            return new SqlServerSproc(_conn, CacheProvider);
        }


    }
}
