using System;
using System.Data.SqlClient;
using SprocMapperLibrary.Base;

namespace SprocMapperLibrary.SqlServer
{
    /// <summary>
    /// 
    /// </summary>
    public class SqlServerAccess
    {
        private readonly string _connectionString;
        private readonly SqlCredential _credential;
        private AbstractCacheProvider _cacheProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        public SqlServerAccess(string connectionString)
        {
            _connectionString = connectionString;
            _credential = null;
            _cacheProvider = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="credential"></param>
        public SqlServerAccess(string connectionString, SqlCredential credential)
        {
            _connectionString = connectionString;
            _credential = credential;
            _cacheProvider = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public SqlServerSproc Sproc()
        {
            SqlConnection conn;
            if (_credential == null && _connectionString != null)
                conn = new SqlConnection(_connectionString);

            else if (_credential != null && _connectionString != null)
                conn = new SqlConnection(_connectionString, _credential);

            else
                throw new ArgumentException("Please ensure that valid Sql Credentials have been passed in.");
            
            return new SqlServerSproc(conn, _cacheProvider);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <typeparam name="T"></typeparam>
        public void RegisterCacheProvider<T>(AbstractCacheProvider cacheProvider)
            where T : AbstractCacheProvider
        {
            if (_cacheProvider == null)
                throw new InvalidOperationException("Cache provider already registered.");

            _cacheProvider = cacheProvider;
        }
    }
}
