using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.Threading.Tasks;
using SprocMapperLibrary.CacheProvider;

namespace SprocMapperLibrary.SqlServer
{
    /// <inheritdoc />
    public class SqlServerQuery : BaseQuery
    {
        private SqlConnection _conn;

        private readonly string _connectionString;

        /// <inheritdoc />
        public SqlServerQuery(string connectionString, AbstractCacheProvider cacheProvider) : base(cacheProvider)
        {
            _connectionString = connectionString;
        }

        /// <inheritdoc />
        protected override IEnumerable<dynamic> ExecuteDynamicReaderImpl(Action<dynamic, List<dynamic>> getObjectDel,
            string command, int? commandTimeout, DbConnection userConn, string cacheKey, Action saveCacheDel, 
            CommandType? commandType)
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

                var result = new List<dynamic>();

                using (var cmd = new SqlCommand(command, _conn))
                {
                    // Set common SqlCommand properties
                    SetCommandProps(cmd, commandTimeout, commandType);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return new List<dynamic>();

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

        /// <inheritdoc />
        protected override async Task<IEnumerable<dynamic>> ExecuteDynamicReaderImplAsync(Action<dynamic, List<dynamic>> getObjectDel,
            string command, int? commandTimeout, DbConnection userConn, string cacheKey, Action saveCacheDel, CommandType? commandType)
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

                var result = new List<dynamic>();

                using (var cmd = new SqlCommand(command, _conn))
                {
                    // Set common SqlCommand properties
                    SetCommandProps(cmd, commandTimeout, commandType);

                    using (var reader = await cmd.ExecuteReaderAsync())
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

        /// <inheritdoc />
        protected override IEnumerable<TResult> ExecuteReaderImpl<TResult>(Action<DbDataReader, List<TResult>> getObjectDel,
            string command, int? commandTimeout, string[] partitionOnArr, bool validateSelectColumns, DbConnection userConn,
            string cacheKey, Action saveCacheDel, CommandType? commandType, bool valueOrStringType = false)
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

                var result = new List<TResult>();
                using (var cmd = new SqlCommand(command, _conn))
                {
                    // Set common SqlCommand properties
                    SetCommandProps(cmd, commandTimeout, commandType);

                    using (var reader = cmd.ExecuteReader())
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
                                SprocMapper.ValidateSelectColumns(columnSchema, SprocObjectMapList, partitionOnOrdinal);

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

        /// <inheritdoc />
        protected override async Task<IEnumerable<TResult>> ExecuteReaderAsyncImpl<TResult>(Action<DbDataReader, List<TResult>> getObjectDel,
            string command, int? commandTimeout, string[] partitionOnArr, bool validateSelectColumns, DbConnection userConn,
            string cacheKey, Action saveCacheDel, CommandType? commandType, bool valueOrStringType = false)
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

                var result = new List<TResult>();

                using (var cmd = new SqlCommand(command, _conn))
                {
                    // Set common SqlCommand properties
                    SetCommandProps(cmd, commandTimeout, commandType);

                    using (var reader = await cmd.ExecuteReaderAsync())
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
                                SprocMapper.ValidateSelectColumns(columnSchema, SprocObjectMapList, partitionOnOrdinal);

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
    }
}
