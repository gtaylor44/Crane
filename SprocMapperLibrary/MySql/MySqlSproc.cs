using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using SprocMapperLibrary.CacheProvider;

namespace SprocMapperLibrary.MySql
{
    /// <inheritdoc />
    /// <summary>
    /// </summary>
    public class MySqlSproc : BaseSproc
    {
        private MySqlConnection _mySqlConn;
        private readonly string _connectionString;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="cacheProvider"></param>
        public MySqlSproc(string connectionString, AbstractCacheProvider cacheProvider) : base(cacheProvider)
        {
            _connectionString = connectionString;
        }

        /// <inheritdoc />
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
        /// <param name="commandType"></param>
        /// <param name="valueOrStringType"></param>
        /// <returns></returns>
        protected override IEnumerable<TResult> ExecuteReaderImpl<TResult>(Action<DbDataReader, List<TResult>> getObjectDel,
            string storedProcedure, int? commandTimeout, string[] partitionOnArr, bool validateSelectColumns, DbConnection unmanagedConn, 
            string cacheKey, Action saveCacheDel, CommandType? commandType, bool valueOrStringType = false)
        {
            var userProvidedConnection = false;

            try
            {
                userProvidedConnection = unmanagedConn != null;

                // Try open connection if not already open.
                if (!userProvidedConnection)                
                    _mySqlConn = new MySqlConnection(_connectionString);
                    
                else
                    _mySqlConn = unmanagedConn as MySqlConnection;

                OpenConn(_mySqlConn);

                List<TResult> result = new List<TResult>();
                using (MySqlCommand command = new MySqlCommand(storedProcedure, _mySqlConn))
                {
                    // Set common SqlCommand properties
                    SetCommandProps(command, commandTimeout, commandType);

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return new List<TResult>();

                        if (!valueOrStringType)
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
                        }

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

        /// <inheritdoc />
        protected override IEnumerable<dynamic> ExecuteDynamicReaderImpl(Action<dynamic, List<dynamic>> getObjectDel,
            string storedProcedure, int? commandTimeout, DbConnection userConn, string cacheKey, Action saveCacheDel, CommandType? commandType)
        {
            var userProvidedConnection = false;
            try
            {
                userProvidedConnection = userConn != null;

                // Try open connection if not already open.
                if (!userProvidedConnection)
                    _mySqlConn = new MySqlConnection(_connectionString);

                else
                    _mySqlConn = userConn as MySqlConnection;

                OpenConn(_mySqlConn);

                List<dynamic> result = new List<dynamic>();

                using (MySqlCommand command = new MySqlCommand(storedProcedure, _mySqlConn))
                {
                    // Set common SqlCommand properties
                    SetCommandProps(command, commandTimeout, commandType);

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return new List<dynamic>();

                        DataTable schema = reader.GetSchemaTable();

                        var dynamicColumnDic = SprocMapper.GetColumnsForDynamicQuery(schema);

                        while (reader.Read())
                        {
                            dynamic expando = new ExpandoObject();

                            foreach (var col in dynamicColumnDic)
                                ((IDictionary<String, object>)expando)[col.Value] = reader[col.Key];

                            getObjectDel(expando, result);
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

        /// <inheritdoc />
        protected override async Task<IEnumerable<dynamic>> ExecuteDynamicReaderImplAsync(Action<dynamic, List<dynamic>> getObjectDel,
            string storedProcedure, int? commandTimeout, DbConnection userConn, string cacheKey, Action saveCacheDel, CommandType? commandType)
        {
            var userProvidedConnection = false;
            try
            {
                userProvidedConnection = userConn != null;

                // Try open connection if not already open.
                if (!userProvidedConnection)
                    _mySqlConn = new MySqlConnection(_connectionString);

                else
                    _mySqlConn = userConn as MySqlConnection;

                await OpenConnAsync(_mySqlConn);

                List<dynamic> result = new List<dynamic>();

                using (MySqlCommand command = new MySqlCommand(storedProcedure, _mySqlConn))
                {
                    // Set common SqlCommand properties
                    SetCommandProps(command, commandTimeout, commandType);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                            return new List<dynamic>();

                        DataTable schema = reader.GetSchemaTable();

                        var dynamicColumnDic = SprocMapper.GetColumnsForDynamicQuery(schema);

                        while (await reader.ReadAsync())
                        {
                            dynamic expando = new ExpandoObject();

                            foreach (var col in dynamicColumnDic)
                                ((IDictionary<String, object>)expando)[col.Value] = reader[col.Key];

                            getObjectDel(expando, result);
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

        /// <inheritdoc />
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
        /// <param name="commandType"></param>
        /// <param name="valueOrStringType"></param>
        /// <returns></returns>
        protected override async Task<IEnumerable<TResult>> ExecuteReaderAsyncImpl<TResult>(Action<DbDataReader, List<TResult>> getObjectDel,
            string storedProcedure, int? commandTimeout, string[] partitionOnArr, bool validateSelectColumns, DbConnection unmanagedConn, 
            string cacheKey, Action saveCacheDel, CommandType? commandType, bool valueOrStringType = false)
        {
            var userProvidedConnection = false;

            try
            {
                userProvidedConnection = unmanagedConn != null;

                // Try open connection if not already open.
                if (!userProvidedConnection)                
                    _mySqlConn = new MySqlConnection(_connectionString);
                                                       
                else
                    _mySqlConn = unmanagedConn as MySqlConnection;

                await OpenConnAsync(_mySqlConn);

                var result = new List<TResult>();

                using (MySqlCommand command = new MySqlCommand(storedProcedure, _mySqlConn))
                {
                    // Set common SqlCommand properties
                    SetCommandProps(command, commandTimeout, commandType);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                            return (List<TResult>)Activator.CreateInstance(typeof(List<TResult>));

                        if (!valueOrStringType)
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
                        }

                        while (await reader.ReadAsync())
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

        /// <inheritdoc />
        /// <summary>
        /// Execute a MySQL stored procedure synchronously.
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="commandType"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="unmanagedConn"></param>
        /// <returns>Number of affected records.</returns>
        public override int ExecuteNonQuery(string storedProcedure, CommandType? commandType = null, int? commandTimeout = null, DbConnection unmanagedConn = null)
        {
            try
            {
                int affectedRecords;

                // Try open connection if not already open.
                if (unmanagedConn == null)                
                    _mySqlConn = new MySqlConnection(_connectionString);
                                   
                else                
                    _mySqlConn = unmanagedConn as MySqlConnection;
                
                OpenConn(_mySqlConn);

                using (MySqlCommand command = new MySqlCommand(storedProcedure, _mySqlConn))
                {
                    SetCommandProps(command, commandTimeout, commandType);
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

        /// <inheritdoc />
        /// <summary>
        /// Execute a stored procedure asynchronously.
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="commandType"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="unmanagedConn"></param>
        /// <returns>Number of affected records.</returns>
        public override async Task<int> ExecuteNonQueryAsync(string storedProcedure, CommandType? commandType = null, int? commandTimeout = null, DbConnection unmanagedConn = null)
        {
            try
            {
                int affectedRecords;

                // Try open connection if not already open.
                if (unmanagedConn == null)             
                    _mySqlConn = new MySqlConnection(_connectionString);
                                                       
                else                
                    _mySqlConn = unmanagedConn as MySqlConnection;
                
                await OpenConnAsync(_mySqlConn);

                using (MySqlCommand command = new MySqlCommand(storedProcedure, _mySqlConn))
                {
                    SetCommandProps(command, commandTimeout, commandType);
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

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="commandType"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="unmanagedConn"></param>
        /// <returns>First column of the first row in the result set.</returns>
        public override T ExecuteScalar<T>(string storedProcedure, CommandType? commandType = null, int? commandTimeout = null, DbConnection unmanagedConn = null)
        {
            try
            {
                T obj;

                // Try open connection if not already open.
                if (unmanagedConn == null)                
                    _mySqlConn = new MySqlConnection(_connectionString);
                                    
                else                
                    _mySqlConn = unmanagedConn as MySqlConnection;
                
                OpenConn(_mySqlConn);

                using (MySqlCommand command = new MySqlCommand(storedProcedure, _mySqlConn))
                {
                    SetCommandProps(command, commandTimeout, commandType);
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

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="commandType"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="unmanagedConn"></param>
        /// <returns>First column of the first row in the result set.</returns>
        public override async Task<T> ExecuteScalarAsync<T>(string storedProcedure, CommandType? commandType = null, int? commandTimeout = null, DbConnection unmanagedConn = null)
        {
            try
            {
                T obj;

                // Try open connection if not already open.
                if (unmanagedConn == null)               
                    _mySqlConn = new MySqlConnection(_connectionString);
                                                 
                else                
                    _mySqlConn = unmanagedConn as MySqlConnection;
                
                await OpenConnAsync(_mySqlConn);

                using (MySqlCommand command = new MySqlCommand(storedProcedure, _mySqlConn))
                {
                    SetCommandProps(command, commandTimeout, commandType);
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
