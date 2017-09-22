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
    public class MySqlUserQuery : BaseQuery
    {
        private MySqlConnection _mySqlConn;
        private readonly string _connectionString;

        /// <inheritdoc />
        public MySqlUserQuery(string connectionString, AbstractCacheProvider cacheProvider) : base(cacheProvider)
        {
            _connectionString = connectionString;
        }

        /// <inheritdoc />
        protected override IEnumerable<TResult> ExecuteReaderImpl<TResult>(
            Action<DbDataReader, List<TResult>> getObjectDel,
            string query, int? commandTimeout, string[] partitionOnArr, bool validateSelectColumns,
            DbConnection unmanagedConn,
            string cacheKey, Action saveCacheDel, bool valueOrStringType = false)
        {
            var userProvidedConnection = false;

            try
            {
                query = GetCleanSqlCommand(query);

                userProvidedConnection = unmanagedConn != null;

                // Try open connection if not already open.
                if (!userProvidedConnection)
                    _mySqlConn = new MySqlConnection(_connectionString);

                else
                    _mySqlConn = unmanagedConn as MySqlConnection;

                OpenConn(_mySqlConn);

                List<TResult> result = new List<TResult>();
                using (MySqlCommand command = new MySqlCommand(query, _mySqlConn))
                {
                    // Set common SqlCommand properties
                    SetCommandProps(command, commandTimeout, query);

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
                                    query);

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
            string query, int? commandTimeout, DbConnection userConn, string cacheKey, Action saveCacheDel)
        {
            var userProvidedConnection = false;
            try
            {
                query = GetCleanSqlCommand(query);

                userProvidedConnection = userConn != null;

                // Try open connection if not already open.
                if (!userProvidedConnection)
                    _mySqlConn = new MySqlConnection(_connectionString);

                else
                    _mySqlConn = userConn as MySqlConnection;

                OpenConn(_mySqlConn);

                var result = new List<dynamic>();

                using (var command = new MySqlCommand(query, _mySqlConn))
                {
                    // Set common SqlCommand properties
                    SetCommandProps(command, commandTimeout, query);

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
                                ((IDictionary<String, object>) expando)[col.Value] = reader[col.Key];

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
        protected override async Task<IEnumerable<dynamic>> ExecuteDynamicReaderImplAsync(
            Action<dynamic, List<dynamic>> getObjectDel,
            string query, int? commandTimeout, DbConnection userConn, string cacheKey, Action saveCacheDel)
        {
            var userProvidedConnection = false;
            try
            {
                query = GetCleanSqlCommand(query);

                userProvidedConnection = userConn != null;

                // Try open connection if not already open.
                if (!userProvidedConnection)
                    _mySqlConn = new MySqlConnection(_connectionString);

                else
                    _mySqlConn = userConn as MySqlConnection;

                await OpenConnAsync(_mySqlConn);

                var result = new List<dynamic>();

                using (var command = new MySqlCommand(query, _mySqlConn))
                {
                    // Set common SqlCommand properties
                    SetCommandProps(command, commandTimeout, query);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                            return new List<dynamic>();

                        var schema = reader.GetSchemaTable();

                        var dynamicColumnDic = SprocMapper.GetColumnsForDynamicQuery(schema);

                        while (await reader.ReadAsync())
                        {
                            dynamic expando = new ExpandoObject();

                            foreach (var col in dynamicColumnDic)
                                ((IDictionary<String, object>) expando)[col.Value] = reader[col.Key];

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
        protected override async Task<IEnumerable<TResult>> ExecuteReaderAsyncImpl<TResult>(
            Action<DbDataReader, List<TResult>> getObjectDel,
            string query, int? commandTimeout, string[] partitionOnArr, bool validateSelectColumns,
            DbConnection unmanagedConn,
            string cacheKey, Action saveCacheDel, bool valueOrStringType = false)
        {
            var userProvidedConnection = false;

            try
            {
                query = GetCleanSqlCommand(query);

                userProvidedConnection = unmanagedConn != null;

                // Try open connection if not already open.
                if (!userProvidedConnection)
                    _mySqlConn = new MySqlConnection(_connectionString);

                else
                    _mySqlConn = unmanagedConn as MySqlConnection;

                await OpenConnAsync(_mySqlConn);

                var result = new List<TResult>();

                using (MySqlCommand command = new MySqlCommand(query, _mySqlConn))
                {
                    // Set common SqlCommand properties
                    SetCommandProps(command, commandTimeout, query);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                            return (List<TResult>) Activator.CreateInstance(typeof(List<TResult>));

                        if (!valueOrStringType)
                        {
                            var schema = reader.GetSchemaTable();
                            var rowList = schema?.Rows.Cast<DataRow>().ToList();

                            int[] partitionOnOrdinal = null;

                            if (partitionOnArr != null)
                                partitionOnOrdinal =
                                    SprocMapper.GetOrdinalPartition(rowList, partitionOnArr, SprocObjectMapList.Count);

                            SprocMapper.SetOrdinal(rowList, SprocObjectMapList, partitionOnOrdinal);

                            if (validateSelectColumns)
                                SprocMapper.ValidateSelectColumns(rowList, SprocObjectMapList, partitionOnOrdinal,
                                    query);

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
        public override T ExecuteScalar<T>(string query, int? commandTimeout = null, DbConnection unmanagedConn = null)
        {
            try
            {
                query = GetCleanSqlCommand(query);

                T obj;

                // Try open connection if not already open.
                if (unmanagedConn == null)
                    _mySqlConn = new MySqlConnection(_connectionString);

                else
                    _mySqlConn = unmanagedConn as MySqlConnection;

                OpenConn(_mySqlConn);

                using (MySqlCommand command = new MySqlCommand(query, _mySqlConn))
                {
                    SetCommandProps(command, commandTimeout, query);
                    obj = (T)command.ExecuteScalar();
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
        public override async Task<T> ExecuteScalarAsync<T>(string query, int? commandTimeout = null, DbConnection unmanagedConn = null)
        {
            try
            {
                query = GetCleanSqlCommand(query);

                T obj;

                // Try open connection if not already open.
                if (unmanagedConn == null)
                    _mySqlConn = new MySqlConnection(_connectionString);

                else
                    _mySqlConn = unmanagedConn as MySqlConnection;

                await OpenConnAsync(_mySqlConn);

                using (MySqlCommand command = new MySqlCommand(query, _mySqlConn))
                {
                    SetCommandProps(command, commandTimeout, query);
                    obj = (T)await command.ExecuteScalarAsync();
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
