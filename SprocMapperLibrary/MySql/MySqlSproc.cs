using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using SprocMapperLibrary.CacheProvider;

namespace SprocMapperLibrary.MySql
{
    /// <summary>
    /// 
    /// </summary>
    public class MySqlSproc : BaseSproc
    {
        private MySqlConnection _mySqlConn;
        private readonly string _connectionString;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="cacheProvider"></param>
        public MySqlSproc(string connectionString, AbstractCacheProvider cacheProvider) : base(cacheProvider)
        {
            _connectionString = connectionString;
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
        /// <param name="unmanagedConn"></param>
        /// <param name="cacheKey"></param>
        /// <param name="saveCacheDel"></param>
        /// <returns></returns>
        protected override IEnumerable<TResult> ExecuteReaderImpl<TResult>(Action<DbDataReader, List<TResult>> getObjectDel,
            string storedProcedure, int? commandTimeout, string[] partitionOnArr, bool validateSelectColumns, DbConnection unmanagedConn, string cacheKey, Action saveCacheDel)
        {
            var userProvidedConnection = false;

            try
            {
                userProvidedConnection = unmanagedConn != null;

                // Try open connection if not already open.
                if (!userProvidedConnection)
                {
                    _mySqlConn = new MySqlConnection(_connectionString);
                    OpenConn(_mySqlConn);
                }
                    
                else
                    _mySqlConn = unmanagedConn as MySqlConnection;

                List<TResult> result = new List<TResult>();
                using (MySqlCommand command = new MySqlCommand(storedProcedure, _mySqlConn))
                {
                    // Set common SqlCommand properties
                    SetCommandProps(command, commandTimeout);

                    using (var reader = command.ExecuteReader())
                    {
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

                        if (!reader.HasRows)
                            return (List<TResult>) Activator.CreateInstance(typeof(List<TResult>));

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
                    _mySqlConn.Dispose();
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
        /// <param name="unmanagedConn"></param>
        /// <param name="cacheKey"></param>
        /// <param name="saveCacheDel"></param>
        /// <returns></returns>
        protected override async Task<IEnumerable<TResult>> ExecuteReaderAsyncImpl<TResult>(Action<DbDataReader, List<TResult>> getObjectDel,
            string storedProcedure, int? commandTimeout, string[] partitionOnArr, bool validateSelectColumns, DbConnection unmanagedConn, string cacheKey, Action saveCacheDel)
        {
            var userProvidedConnection = false;

            try
            {
                userProvidedConnection = unmanagedConn != null;

                // Try open connection if not already open.
                if (!userProvidedConnection)
                {
                    _mySqlConn = new MySqlConnection(_connectionString);
                    await OpenConnAsync(_mySqlConn);
                }
                    
                else
                    _mySqlConn = unmanagedConn as MySqlConnection;

                List<TResult> result = new List<TResult>();

                using (MySqlCommand command = new MySqlCommand(storedProcedure, _mySqlConn))
                {
                    // Set common SqlCommand properties
                    SetCommandProps(command, commandTimeout);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
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

                        SprocMapper.ValidateSchema(schema, SprocObjectMapList , partitionOnOrdinal);

                        if (!reader.HasRows)
                            return (List<TResult>) Activator.CreateInstance(typeof(List<TResult>));

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
                    _mySqlConn.Dispose();
            }
        }

        /// <summary>
        /// Execute a MySQL stored procedure synchronously.
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="unmanagedConn"></param>
        /// <returns>Number of affected records.</returns>
        public override int ExecuteNonQuery(string storedProcedure, int? commandTimeout = null, DbConnection unmanagedConn = null)
        {
            try
            {
                int affectedRecords;

                // Try open connection if not already open.
                if (unmanagedConn == null)
                {
                    _mySqlConn = new MySqlConnection(_connectionString);
                    OpenConn(_mySqlConn);
                }
                    
                else
                {
                    _mySqlConn = unmanagedConn as MySqlConnection;
                }

                using (MySqlCommand command = new MySqlCommand(storedProcedure, _mySqlConn))
                {
                    SetCommandProps(command, commandTimeout);
                    affectedRecords = command.ExecuteNonQuery();
                }

                return affectedRecords;
            }
            finally
            {
                if (unmanagedConn == null)
                    _mySqlConn.Dispose();
            }

        }

        /// <summary>
        /// Execute a stored procedure asynchronously.
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="unmanagedConn"></param>
        /// <returns>Number of affected records.</returns>
        public override async Task<int> ExecuteNonQueryAsync(string storedProcedure, int? commandTimeout = null, DbConnection unmanagedConn = null)
        {
            try
            {
                int affectedRecords;

                // Try open connection if not already open.
                if (unmanagedConn == null)
                {
                    _mySqlConn = new MySqlConnection(_connectionString);
                    await OpenConnAsync(_mySqlConn);
                }
                    
                else
                {
                    _mySqlConn = unmanagedConn as MySqlConnection;
                }

                using (MySqlCommand command = new MySqlCommand(storedProcedure, _mySqlConn))
                {
                    SetCommandProps(command, commandTimeout);
                    affectedRecords = await command.ExecuteNonQueryAsync();
                }

                return affectedRecords;
            }
            finally
            {
                if (unmanagedConn == null)
                     _mySqlConn.Dispose();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="unmanagedConn"></param>
        /// <returns>First column of the first row in the result set.</returns>
        public override T ExecuteScalar<T>(string storedProcedure, int? commandTimeout = null, DbConnection unmanagedConn = null)
        {
            try
            {
                T obj;

                // Try open connection if not already open.
                if (unmanagedConn == null)
                {
                    _mySqlConn = new MySqlConnection(_connectionString);
                    OpenConn(_mySqlConn);
                }
                    
                else
                {
                    _mySqlConn = unmanagedConn as MySqlConnection;
                }

                using (MySqlCommand command = new MySqlCommand(storedProcedure, _mySqlConn))
                {
                    SetCommandProps(command, commandTimeout);
                    obj = (T) command.ExecuteScalar();
                }

                return obj;
            }
            finally
            {
                if (unmanagedConn == null)
                    _mySqlConn.Dispose();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="unmanagedConn"></param>
        /// <returns>First column of the first row in the result set.</returns>
        public override async Task<T> ExecuteScalarAsync<T>(string storedProcedure, int? commandTimeout = null, DbConnection unmanagedConn = null)
        {
            try
            {
                T obj;

                // Try open connection if not already open.
                if (unmanagedConn == null)
                {
                    _mySqlConn = new MySqlConnection(_connectionString);
                    await OpenConnAsync(_mySqlConn);
                }
                    
                else
                {
                    _mySqlConn = unmanagedConn as MySqlConnection;
                }

                using (MySqlCommand command = new MySqlCommand(storedProcedure, _mySqlConn))
                {
                    SetCommandProps(command, commandTimeout);
                    obj = (T) await command.ExecuteScalarAsync();
                }

                return obj;
            }

            finally
            {
                if (unmanagedConn == null)
                    _mySqlConn.Dispose();
            }

        }
    }
}
