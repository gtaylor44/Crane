﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace Crane.SqlServer
{
    /// <inheritdoc />
    public class SqlServerQuery : BaseQuery
    {
        private SqlConnection _conn;

        private readonly string _connectionString;

#if NETFRAMEWORK
        private readonly SqlCredential _credential;
#endif


#if NETFRAMEWORK
        /// <inheritdoc />
        public SqlServerQuery(string connectionString, SqlCredential credential,
            QueryOptions options) : base(options)
        {
            _connectionString = connectionString;
            _credential = credential;
        }
#elif NETCORE
        /// <inheritdoc />
        public SqlServerQuery(string connectionString, QueryOptions options) : base(options)
        {
            _connectionString = connectionString;
        }
#endif
        /// <inheritdoc />
        protected override IEnumerable<dynamic> ExecuteDynamicReaderImpl(Action<dynamic, List<dynamic>> getObjectDel,
            string query, int? commandTimeout, DbConnection userConn, DbTransaction transaction, string cacheKey, Action saveCacheDel)
        {
            var userProvidedConnection = false;
            try
            {
                query = GetCleanSqlCommand(query);

                userProvidedConnection = userConn != null;

                // Try open connection if not already open.
                _conn = GetSqlConnection(userConn, userProvidedConnection);

                OpenConn(_conn);

                var result = new List<dynamic>();

                using (var cmd = new SqlCommand(query, _conn))
                {
                    // Set common SqlCommand properties
                    SetCommandProps(cmd, transaction, commandTimeout, query);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return new List<dynamic>();

                        var schema = reader.GetSchemaTable();

                        var dynamicColumnDic = CraneHelper.GetColumnsForDynamicQuery(schema);

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
            string query, int? commandTimeout, DbConnection userConn, DbTransaction transaction, string cacheKey, Action saveCacheDel)
        {
            var userProvidedConnection = false;
            try
            {
                query = GetCleanSqlCommand(query);

                userProvidedConnection = userConn != null;

                // Try open connection if not already open.
                _conn = GetSqlConnection(userConn, userProvidedConnection);

                await OpenConnAsync(_conn);

                var result = new List<dynamic>();

                using (var cmd = new SqlCommand(query, _conn))
                {
                    // Set common SqlCommand properties
                    SetCommandProps(cmd, transaction, commandTimeout, query);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                            return new List<dynamic>();

                        var schema = reader.GetSchemaTable();
                        var dynamicColumnDic = CraneHelper.GetColumnsForDynamicQuery(schema);

                        while (await reader.ReadAsync())
                        {
                            dynamic expando = new ExpandoObject();

                            foreach (var col in dynamicColumnDic)
                                ((IDictionary<string, object>)expando)[col.Value] = reader[col.Key];

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
            string query, int? commandTimeout, string[] partitionOnArr, bool validateSelectColumns, DbConnection userConn, DbTransaction transaction,
            string cacheKey, Action saveCacheDel, bool valueOrStringType = false)
        {
            var userProvidedConnection = false;
            try
            {
                query = GetCleanSqlCommand(query);

                userProvidedConnection = userConn != null;

                // Try open connection if not already open.
                _conn = GetSqlConnection(userConn, userProvidedConnection);

                OpenConn(_conn);

                var result = new List<TResult>();
                using (SqlCommand cmd = new SqlCommand(query, _conn))
                {
                    // Set common SqlCommand properties
                    SetCommandProps(cmd, transaction, commandTimeout, query);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return new List<TResult>();

                        if (!valueOrStringType)
                        {
                            var schema = reader.GetSchemaTable();
                            var rowList = schema?.Rows.Cast<DataRow>().ToList();

                            int[] partitionOnOrdinal = null;

                            if (partitionOnArr != null)
                                partitionOnOrdinal =
                                    CraneHelper.GetOrdinalPartition(rowList, partitionOnArr, SprocObjectMapList.Count);

                            CraneHelper.SetOrdinal(rowList, SprocObjectMapList, partitionOnOrdinal);

                            if (validateSelectColumns)
                                CraneHelper.ValidateSelectColumns(rowList, SprocObjectMapList, partitionOnOrdinal,
                                    query);

                            CraneHelper.ValidateSchema(schema, SprocObjectMapList, partitionOnOrdinal);
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
            string query, int? commandTimeout, string[] partitionOnArr, bool validateSelectColumns, DbConnection userConn, DbTransaction transaction,
            string cacheKey, Action saveCacheDel, bool valueOrStringType = false)
        {
            var userProvidedConnection = false;
            try
            {
                query = GetCleanSqlCommand(query);

                userProvidedConnection = userConn != null;

                // Try open connection if not already open.
                _conn = GetSqlConnection(userConn, userProvidedConnection);

                await OpenConnAsync(_conn);

                var result = new List<TResult>();

                using (var cmd = new SqlCommand(query, _conn))
                {
                    // Set common SqlCommand properties
                    SetCommandProps(cmd, transaction, commandTimeout, query);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                            return new List<TResult>();

                        if (!valueOrStringType)
                        {
                            var schema = reader.GetSchemaTable();
                            var rowList = schema?.Rows.Cast<DataRow>().ToList();

                            int[] partitionOnOrdinal = null;

                            if (partitionOnArr != null)
                                partitionOnOrdinal =
                                    CraneHelper.GetOrdinalPartition(rowList, partitionOnArr, SprocObjectMapList.Count);

                            CraneHelper.SetOrdinal(rowList, SprocObjectMapList, partitionOnOrdinal);

                            if (validateSelectColumns)
                                CraneHelper.ValidateSelectColumns(rowList, SprocObjectMapList, partitionOnOrdinal,
                                    query);

                            CraneHelper.ValidateSchema(schema, SprocObjectMapList, partitionOnOrdinal);
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

        /// <inheritdoc />
        public override T ExecuteScalar<T>(string query, int? commandTimeout = null, DbConnection userConn = null, DbTransaction transaction = null)
        {
            try
            {
                query = GetCleanSqlCommand(query);

                T obj;

                if (userConn == null)
                    _conn = new SqlConnection(_connectionString);

                else
                    _conn = userConn as SqlConnection;

                OpenConn(_conn);

                using (var cmd = new SqlCommand(query, _conn))
                {
                    SetCommandProps(cmd, transaction, commandTimeout, query);
                    obj = (T)cmd.ExecuteScalar();
                }

                return obj;
            }

            finally
            {
                if (userConn == null)
                    _conn.Dispose();
            }
        }

        /// <inheritdoc />
        public override async Task<T> ExecuteScalarAsync<T>(string query, int? commandTimeout = null, DbConnection userConn = null, DbTransaction transaction = null)
        {
            try
            {
                query = GetCleanSqlCommand(query);

                T obj;

                if (userConn == null)
                    _conn = new SqlConnection(_connectionString);

                else
                    _conn = userConn as SqlConnection;

                await OpenConnAsync(_conn);

                using (var cmd = new SqlCommand(query, _conn))
                {
                    SetCommandProps(cmd, transaction, commandTimeout, query);
                    obj = (T)await cmd.ExecuteScalarAsync();
                }

                return obj;
            }

            finally
            {
                if (userConn == null)
                    _conn.Dispose();
            }

        }

        private SqlConnection GetSqlConnection(DbConnection userConn, bool userProvidedConnection)
        {

            SqlConnection conn;
#if NETFRAMEWORK
                if (!userProvidedConnection)
                    conn = _credential == null ? new SqlConnection(_connectionString)
                        : new SqlConnection(_connectionString, _credential);
                else
                    conn = userConn as SqlConnection;
#elif NETCORE
            if (!userProvidedConnection)
                conn = new SqlConnection(_connectionString);
            else
                conn = userConn as SqlConnection;
#endif
            return conn;
        }
    }
}
