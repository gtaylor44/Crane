using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using FastMember;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="credential"></param>
        /// <param name="cacheProvider"></param>
        public SqlServerSproc(string connectionString, AbstractCacheProvider cacheProvider) : base(cacheProvider)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getObjectDel"></param>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="userConn"></param>
        /// <param name="cacheKey"></param>
        /// <param name="saveCacheDel"></param>
        /// <param name="commandType"></param>
        protected override IEnumerable<dynamic> ExecuteDynamicReaderImpl(Action<dynamic, List<dynamic>> getObjectDel,
            string storedProcedure, int? commandTimeout, DbConnection userConn, string cacheKey, Action saveCacheDel, 
            CommandType commandType)
        {
            var userProvidedConnection = false;
            try
            {
                userProvidedConnection = userConn != null;

                // Try open connection if not already open.
                if (!userProvidedConnection)
                    _conn = new SqlConnection(_connectionString);

                else
                    _conn = userConn as SqlConnection;

                OpenConn(_conn);

                List<dynamic> result = new List<dynamic>();

                using (SqlCommand command = new SqlCommand(storedProcedure, _conn))
                {
                    // Set common SqlCommand properties
                    SetCommandProps(command, commandTimeout, commandType);

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return new List<dynamic>();

                        var test = reader.GetColumnSchema();

                        if (!reader.CanGetColumnSchema())
                            throw new SprocMapperException("Could not get column schema for table");

                        var columnSchema = reader.GetColumnSchema();

                        var dynamicColumnDic = SprocMapper.GetColumnsForDynamicQuery(columnSchema);

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
                    _conn.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getObjectDel"></param>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="userConn"></param>
        /// <param name="cacheKey"></param>
        /// <param name="saveCacheDel"></param>
        /// <param name="commandType"></param>
        protected override async Task<IEnumerable<dynamic>> ExecuteDynamicReaderImplAsync(Action<dynamic, List<dynamic>> getObjectDel,
            string storedProcedure, int? commandTimeout, DbConnection userConn, string cacheKey, Action saveCacheDel, CommandType commandType)
        {
            var userProvidedConnection = false;
            try
            {
                userProvidedConnection = userConn != null;

                // Try open connection if not already open.
                if (!userProvidedConnection)
                    _conn = new SqlConnection(_connectionString);

                else
                    _conn = userConn as SqlConnection;

                await OpenConnAsync(_conn);

                List<dynamic> result = new List<dynamic>();

                using (SqlCommand command = new SqlCommand(storedProcedure, _conn))
                {
                    // Set common SqlCommand properties
                    SetCommandProps(command, commandTimeout, commandType);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                            return new List<dynamic>();

                        if (!reader.CanGetColumnSchema())
                            throw new SprocMapperException("Could not get column schema for table");

                        var columnSchema = reader.GetColumnSchema();

                        var dynamicColumnDic = SprocMapper.GetColumnsForDynamicQuery(columnSchema);

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
                    _conn.Dispose();
            }
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
        /// <param name="commandType"></param>
        /// <param name="valueOrStringType"></param>
        /// <returns></returns>
        protected override IEnumerable<TResult> ExecuteReaderImpl<TResult>(Action<DbDataReader, List<TResult>> getObjectDel,
            string storedProcedure, int? commandTimeout, string[] partitionOnArr, bool validateSelectColumns, DbConnection userConn,
            string cacheKey, Action saveCacheDel, CommandType commandType, bool valueOrStringType = false)
        {
            var userProvidedConnection = false;
            try
            {
                userProvidedConnection = userConn != null;

                // Try open connection if not already open.
                if (!userProvidedConnection)
                    _conn = new SqlConnection(_connectionString);

                else
                    _conn = userConn as SqlConnection;

                OpenConn(_conn);

                List<TResult> result = new List<TResult>();
                using (SqlCommand command = new SqlCommand(storedProcedure, _conn))
                {
                    // Set common SqlCommand properties
                    SetCommandProps(command, commandTimeout, commandType);

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return new List<TResult>();

                        if (!valueOrStringType)
                        {
                            if (!reader.CanGetColumnSchema())
                                throw new SprocMapperException("Could not get column schema for table");

                            var columnSchema = reader.GetColumnSchema();

                            int[] partitionOnOrdinal = null;

                            if (partitionOnArr != null)
                                partitionOnOrdinal =
                                    SprocMapper.GetOrdinalPartition(columnSchema, partitionOnArr, SprocObjectMapList.Count);

                            SprocMapper.SetOrdinal(columnSchema, SprocObjectMapList, partitionOnOrdinal);

                            if (validateSelectColumns)
                                SprocMapper.ValidateSelectColumns(columnSchema, SprocObjectMapList, partitionOnOrdinal,
                                    storedProcedure);

                            SprocMapper.ValidateSchema(columnSchema, SprocObjectMapList, partitionOnOrdinal);
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
        /// <param name="commandType"></param>
        /// <param name="valueOrStringType"></param>
        /// <returns></returns>
        protected override async Task<IEnumerable<TResult>> ExecuteReaderAsyncImpl<TResult>(Action<DbDataReader, List<TResult>> getObjectDel,
            string storedProcedure, int? commandTimeout, string[] partitionOnArr, bool validateSelectColumns, DbConnection userConn,
            string cacheKey, Action saveCacheDel, CommandType commandType, bool valueOrStringType = false)
        {
            var userProvidedConnection = false;
            try
            {
                userProvidedConnection = userConn != null;

                // Try open connection if not already open.
                if (!userProvidedConnection)
                    _conn = new SqlConnection(_connectionString);

                else
                    _conn = userConn as SqlConnection;

                await OpenConnAsync(_conn);

                List<TResult> result = new List<TResult>();

                using (SqlCommand command = new SqlCommand(storedProcedure, _conn))
                {
                    // Set common SqlCommand properties
                    SetCommandProps(command, commandTimeout, commandType);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                            return new List<TResult>();

                        if (!valueOrStringType)
                        {
                            if (!reader.CanGetColumnSchema())
                                throw new SprocMapperException("Could not get column schema for table");

                            var columnSchema = reader.GetColumnSchema();

                            int[] partitionOnOrdinal = null;

                            if (partitionOnArr != null)
                                partitionOnOrdinal =
                                    SprocMapper.GetOrdinalPartition(columnSchema, partitionOnArr, SprocObjectMapList.Count);

                            SprocMapper.SetOrdinal(columnSchema, SprocObjectMapList, partitionOnOrdinal);

                            if (validateSelectColumns)
                                SprocMapper.ValidateSelectColumns(columnSchema, SprocObjectMapList, partitionOnOrdinal,
                                    storedProcedure);

                            SprocMapper.ValidateSchema(columnSchema, SprocObjectMapList, partitionOnOrdinal);
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
                    _conn.Dispose();
            }
        }

        /// <summary>
        /// Execute a MSSql stored procedure synchronously.
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="commandType"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="userConn"></param>
        /// <returns>Number of affected records.</returns>
        public override int ExecuteNonQuery(string storedProcedure, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null, DbConnection userConn = null)
        {
            try
            {
                int affectedRecords;

                if (userConn == null)
                    _conn = new SqlConnection(_connectionString);

                else
                    _conn = userConn as SqlConnection;

                OpenConn(_conn);

                using (SqlCommand command = new SqlCommand(storedProcedure, _conn))
                {
                    SetCommandProps(command, commandTimeout, commandType);
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
        /// <param name="commandType"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="userConn"></param>
        /// <returns>Number of affected records.</returns>
        public override async Task<int> ExecuteNonQueryAsync(string storedProcedure, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null, DbConnection userConn = null)
        {
            try
            {
                int affectedRecords;

                if (userConn == null)
                    _conn = new SqlConnection(_connectionString);

                else
                    _conn = userConn as SqlConnection;

                await OpenConnAsync(_conn);

                using (SqlCommand command = new SqlCommand(storedProcedure, _conn))
                {
                    SetCommandProps(command, commandTimeout, commandType);
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
        /// <param name="commandType"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="userConn"></param>
        /// <returns>First column of the first row in the result set.</returns>
        public override T ExecuteScalar<T>(string storedProcedure, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null, DbConnection userConn = null)
        {
            try
            {
                T obj;

                if (userConn == null)
                    _conn = new SqlConnection(_connectionString);

                else
                    _conn = userConn as SqlConnection;

                OpenConn(_conn);

                using (SqlCommand command = new SqlCommand(storedProcedure, _conn))
                {
                    SetCommandProps(command, commandTimeout, commandType);
                    obj = (T)command.ExecuteScalar();
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
        /// <param name="commandType"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="userConn"></param>
        /// <returns>First column of the first row in the result set.</returns>
        public override async Task<T> ExecuteScalarAsync<T>(string storedProcedure, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null, DbConnection userConn = null)
        {
            try
            {
                T obj;

                if (userConn == null)
                    _conn = new SqlConnection(_connectionString);

                else
                    _conn = userConn as SqlConnection;

                await OpenConnAsync(_conn);

                using (SqlCommand command = new SqlCommand(storedProcedure, _conn))
                {
                    SetCommandProps(command, commandTimeout, commandType);
                    obj = (T)await command.ExecuteScalarAsync();
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
