using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using SprocMapperLibrary.Base;

namespace SprocMapperLibrary.MySql
{
    /// <summary>
    /// 
    /// </summary>
    public class MySqlSproc : BaseSproc
    {
        private MySqlConnection _mySqlConn;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mySqlConn"></param>
        /// <param name="cacheProvider"></param>
        public MySqlSproc(MySqlConnection mySqlConn, AbstractCacheProvider cacheProvider) : base(cacheProvider)
        {
            _mySqlConn = mySqlConn;
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
        /// <returns></returns>
        protected override IEnumerable<TResult> ExecuteReaderImpl<TResult>(Action<DbDataReader, List<TResult>> getObjectDel,
            string storedProcedure, int? commandTimeout, string[] partitionOnArr, bool validateSelectColumns, DbConnection userConn, string cacheKey)
        {
            var useCache = false;
            var userProvidedConnection = false;

            try
            {
                ValidateCacheKey(cacheKey);
                IEnumerable<TResult> cachedResult;
                if (cacheKey != null && CacheProvider.TryGet(cacheKey, out cachedResult))
                {
                    useCache = true;
                    return cachedResult;
                }

                userProvidedConnection = userConn != null;

                // Try open connection if not already open.
                if (!userProvidedConnection)
                    OpenConn(_mySqlConn);
                else
                    _mySqlConn = userConn as MySqlConnection;

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

                        SprocMapper.ValidateSchema(schema, SprocObjectMapList);

                        if (!reader.HasRows)
                            return (List<TResult>) Activator.CreateInstance(typeof(List<TResult>));

                        while (reader.Read())
                        {
                            getObjectDel(reader, result);
                        }
                    }
                }

                if (cacheKey != null)
                    CacheProvider.Add(cacheKey, result);

                return result;
            }
            finally
            {
                if (!userProvidedConnection && !useCache)
                    _mySqlConn.Close();
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
        /// <returns></returns>
        protected override async Task<IEnumerable<TResult>> ExecuteReaderAsyncImpl<TResult>(Action<DbDataReader, List<TResult>> getObjectDel,
            string storedProcedure, int? commandTimeout, string[] partitionOnArr, bool validateSelectColumns, DbConnection userConn, string cacheKey)
        {
            var useCache = false;
            var userProvidedConnection = false;

            try
            {
                ValidateCacheKey(cacheKey);
                IEnumerable<TResult> cachedResult;
                if (cacheKey != null && CacheProvider.TryGet(cacheKey, out cachedResult))
                {
                    useCache = true;
                    return cachedResult;
                }

                userProvidedConnection = userConn != null;

                // Try open connection if not already open.
                if (!userProvidedConnection)
                    await OpenConnAsync(_mySqlConn);
                else
                    _mySqlConn = userConn as MySqlConnection;

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

                        SprocMapper.ValidateSchema(schema, SprocObjectMapList);

                        if (!reader.HasRows)
                            return (List<TResult>) Activator.CreateInstance(typeof(List<TResult>));

                        while (reader.Read())
                        {
                            getObjectDel(reader, result);
                        }
                    }

                }

                if (cacheKey != null)
                    CacheProvider.Add(cacheKey, result);

                return result;
            }

            finally
            {
                if (!userProvidedConnection && !useCache)
                    await _mySqlConn.CloseAsync();
            }
        }

        /// <summary>
        /// Execute a MySQL stored procedure synchronously.
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

                // Try open connection if not already open.
                if (userConn == null)
                    OpenConn(_mySqlConn);
                else
                {
                    _mySqlConn = userConn as MySqlConnection;
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
                if (userConn == null)
                    _mySqlConn.Close();
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

                // Try open connection if not already open.
                if (userConn == null)
                    await OpenConnAsync(_mySqlConn);
                else
                {
                    _mySqlConn = userConn as MySqlConnection;
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
                if (userConn == null)
                    await _mySqlConn.CloseAsync();
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

                // Try open connection if not already open.
                if (userConn == null)
                    OpenConn(_mySqlConn);
                else
                {
                    _mySqlConn = userConn as MySqlConnection;
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
                if (userConn == null)
                    _mySqlConn.Close();
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

                // Try open connection if not already open.
                if (userConn == null)
                    await OpenConnAsync(_mySqlConn);
                else
                {
                    _mySqlConn = userConn as MySqlConnection;
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
                if (userConn == null)
                    await _mySqlConn.CloseAsync();
            }

        }
    }
}
