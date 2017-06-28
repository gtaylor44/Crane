using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using SprocMapperLibrary.CacheProvider;

namespace SprocMapperLibrary.SqlServer
{
    /// <summary>
    /// 
    /// </summary>
    public class SqlServerSproc : BaseSproc
    {
        private SqlConnection _conn;

        private readonly string _connectionString;
        private readonly SqlCredential _credential;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="credential"></param>
        /// <param name="cacheProvider"></param>
        public SqlServerSproc(string connectionString, SqlCredential credential,
            AbstractCacheProvider cacheProvider) : base(cacheProvider)
        {
            _connectionString = connectionString;
            _credential = credential;
        }

        /// <summary>
        /// Performs synchronous version of stored procedure.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="getObjectDel"></param>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="commandTimeout"></param>
        /// <param name="partitionOnArr"></param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="userConn"></param>
        /// <param name="cacheKey"></param>
        /// <param name="saveCacheDel"></param>
        /// <returns></returns>
        protected override IEnumerable<TResult> ExecuteReaderImpl<TResult>(Action<DbDataReader, List<TResult>> getObjectDel,
            string storedProcedure, int? commandTimeout, string[] partitionOnArr, bool validateSelectColumns, DbConnection userConn, string cacheKey, Action saveCacheDel)
        {
            var userProvidedConnection = false;
            try
            {
                userProvidedConnection = userConn != null;

                // Try open connection if not already open.
                if (!userProvidedConnection)               
                    _conn = _credential == null ? new SqlConnection(_connectionString) 
                        : new SqlConnection(_connectionString, _credential);
                                       
                else              
                    _conn = userConn as SqlConnection;

                OpenConn(_conn);

                List<TResult> result = new List<TResult>();
                using (SqlCommand command = new SqlCommand(storedProcedure, _conn))
                {
                    // Set common SqlCommand properties
                    SetCommandProps(command, commandTimeout);

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return (List<TResult>)Activator.CreateInstance(typeof(List<TResult>));

                        DataTable schema = reader.GetSchemaTable();
                        var rowList = schema?.Rows.Cast<DataRow>().ToList();

                        int[] partitionOnOrdinal = null;

                        if (partitionOnArr != null)
                            partitionOnOrdinal =
                                SprocMapper.GetOrdinalPartition(rowList, partitionOnArr, SprocObjectMapList.Count);

                        SprocMapper.SetOrdinal(rowList, SprocObjectMapList, partitionOnOrdinal);

                        if (validateSelectColumns)
                            SprocMapper.ValidateSelectColumns(rowList, SprocObjectMapList, partitionOnOrdinal,
                                storedProcedure);

                        SprocMapper.ValidateSchema(schema, SprocObjectMapList, partitionOnOrdinal);

                        while (reader.Read())
                        {
                            getObjectDel(reader, result);
                        }
                    }
                }

                if (cacheKey != null)
                    saveCacheDel();

                return result;
            }

            finally
            {
                if (!userProvidedConnection)
                    _conn.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getStringDel"></param>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="partitionOnArr"></param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="userConn"></param>
        /// <param name="cacheKey"></param>
        /// <param name="saveCacheDel"></param>
        /// <returns></returns>
        protected IEnumerable<string> ExecuteReaderImpl(Action<DbDataReader, List<string>> getStringDel, string storedProcedure, int? commandTimeout, string[] partitionOnArr, bool validateSelectColumns, DbConnection userConn, string cacheKey, Action saveCacheDel)
        {
            var userProvidedConnection = false;
            try
            {
                userProvidedConnection = userConn != null;

                // Try open connection if not already open.
                if (!userProvidedConnection)
                    _conn = _credential == null ? new SqlConnection(_connectionString)
                        : new SqlConnection(_connectionString, _credential);

                else
                    _conn = userConn as SqlConnection;

                OpenConn(_conn);

                List<string> result = new List<string>();
                using (SqlCommand command = new SqlCommand(storedProcedure, _conn))
                {
                    // Set common SqlCommand properties
                    SetCommandProps(command, commandTimeout);

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return new List<string>();

                        DataTable schema = reader.GetSchemaTable();
                        //var rowList = schema?.Rows.Cast<DataRow>().ToList();

                        //if (validateSelectColumns)
                        //    SprocMapper.ValidateSelectColumns(rowList, SprocObjectMapList, partitionOnOrdinal,
                        //        storedProcedure);

                        //SprocMapper.ValidateSchema(schema, SprocObjectMapList, partitionOnOrdinal);

                        while (reader.Read())
                        {
                            getStringDel(reader, result);
                        }
                    }
                }

                if (cacheKey != null)
                    saveCacheDel();

                return result;
            }

            finally
            {
                if (!userProvidedConnection)
                    _conn.Dispose();
            }
        }

        /// <summary>
        /// Performs asynchronous version of stored procedure.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="getObjectDel"></param>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="partitionOnArr"></param>
        /// <param name="validateSelectColumns"></param>
        /// <param name="userConn"></param>
        /// <param name="cacheKey"></param>
        /// <param name="saveCacheDel"></param>
        /// <returns></returns>
        protected override async Task<IEnumerable<TResult>> ExecuteReaderAsyncImpl<TResult>(Action<DbDataReader, List<TResult>> getObjectDel,
            string storedProcedure, int? commandTimeout, string[] partitionOnArr, bool validateSelectColumns, DbConnection userConn, string cacheKey, Action saveCacheDel)
        {
            var userProvidedConnection = false;
            try
            {
                userProvidedConnection = userConn != null;

                // Try open connection if not already open.
                if (!userProvidedConnection)                
                    _conn = _credential == null ? new SqlConnection(_connectionString)
                        : new SqlConnection(_connectionString, _credential);                  
                                 
                else               
                    _conn = userConn as SqlConnection;

                await OpenConnAsync(_conn);

                List<TResult> result = new List<TResult>();

                using (SqlCommand command = new SqlCommand(storedProcedure, _conn))
                {
                    // Set common SqlCommand properties
                    SetCommandProps(command, commandTimeout);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                            return (List<TResult>)Activator.CreateInstance(typeof(List<TResult>));

                        DataTable schema = reader.GetSchemaTable();
                        var rowList = schema?.Rows.Cast<DataRow>().ToList();

                        int[] partitionOnOrdinal = null;

                        if (partitionOnArr != null)
                            partitionOnOrdinal =
                                SprocMapper.GetOrdinalPartition(rowList, partitionOnArr, SprocObjectMapList.Count);

                        SprocMapper.SetOrdinal(rowList, SprocObjectMapList, partitionOnOrdinal);

                        if (validateSelectColumns)
                            SprocMapper.ValidateSelectColumns(rowList, SprocObjectMapList, partitionOnOrdinal,
                                storedProcedure);

                        SprocMapper.ValidateSchema(schema, SprocObjectMapList, partitionOnOrdinal);

                        while (reader.Read())
                        {
                            getObjectDel(reader, result);
                        }
                    }
                }

                if (cacheKey != null)
                    saveCacheDel();

                return result;
            }

            finally
            {
                if (!userProvidedConnection)
                    _conn.Dispose();
            }
        }

        /// <summary>
        /// Execute a MSSql stored procedure synchronously.
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="userConn"></param>
        /// <returns>Number of affected records.</returns>
        public override int ExecuteNonQuery(string storedProcedure, int? commandTimeout = null, DbConnection userConn = null)
        {
            try
            {
                int affectedRecords;

                if (userConn == null)                
                    _conn = _credential == null ? new SqlConnection(_connectionString)
                        : new SqlConnection(_connectionString, _credential);                    
                  
                else                
                    _conn = userConn as SqlConnection;

                OpenConn(_conn);

                using (SqlCommand command = new SqlCommand(storedProcedure, _conn))
                {
                    SetCommandProps(command, commandTimeout);
                    affectedRecords = command.ExecuteNonQuery();
                }

                return affectedRecords;
            }
            finally
            {
                if (userConn == null)
                    _conn.Dispose();
            }
        }

        /// <summary>
        /// Execute a stored procedure asynchronously.
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="userConn"></param>
        /// <returns>Number of affected records.</returns>
        public override async Task<int> ExecuteNonQueryAsync(string storedProcedure, int? commandTimeout = null, DbConnection userConn = null)
        {
            try
            {
                int affectedRecords;

                if (userConn == null)   
                    _conn = _credential == null ? new SqlConnection(_connectionString)
                        : new SqlConnection(_connectionString, _credential);
    
                else                
                    _conn = userConn as SqlConnection;

                await OpenConnAsync(_conn);

                using (SqlCommand command = new SqlCommand(storedProcedure, _conn))
                {
                    SetCommandProps(command, commandTimeout);
                    affectedRecords = await command.ExecuteNonQueryAsync();
                }

                return affectedRecords;
            }
            finally
            {
                if (userConn == null)
                    _conn.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="userConn"></param>
        /// <returns>First column of the first row in the result set.</returns>
        public override T ExecuteScalar<T>(string storedProcedure, int? commandTimeout = null, DbConnection userConn = null)
        {
            try
            {
                T obj;

                if (userConn == null)                
                    _conn = _credential == null ? new SqlConnection(_connectionString)
                        : new SqlConnection(_connectionString, _credential);
                   
                else
                    _conn = userConn as SqlConnection;

                OpenConn(_conn);

                using (SqlCommand command = new SqlCommand(storedProcedure, _conn))
                {
                    SetCommandProps(command, commandTimeout);
                    obj = (T) command.ExecuteScalar();
                }

                return obj;
            }

            finally
            {
                if (userConn == null)
                    _conn.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="userConn"></param>
        /// <returns>First column of the first row in the result set.</returns>
        public override async Task<T> ExecuteScalarAsync<T>(string storedProcedure, int? commandTimeout = null, DbConnection userConn = null)
        {
            try
            {
                T obj;

                if (userConn == null)                
                    _conn = _credential == null ? new SqlConnection(_connectionString)
                        : new SqlConnection(_connectionString, _credential);
           
                else              
                    _conn = userConn as SqlConnection;

                await OpenConnAsync(_conn);

                using (SqlCommand command = new SqlCommand(storedProcedure, _conn))
                {
                    SetCommandProps(command, commandTimeout);
                    obj = (T) await command.ExecuteScalarAsync();
                }

                return obj;
            }

            finally
            {
                if (userConn == null)
                    _conn.Dispose();
            }

        }
    }
}
