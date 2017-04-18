using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using SprocMapperLibrary.Base;

namespace SprocMapperLibrary.SqlServer
{
    /// <summary>
    /// 
    /// </summary>
    public class SqlServerSproc : BaseSproc
    {
        private SqlConnection _conn;
        private AbstractCacheProvider _cacheProvider;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="cacheProvider"></param>
        public SqlServerSproc(SqlConnection conn, AbstractCacheProvider cacheProvider) : base()
        {
            _conn = conn;
            _cacheProvider = cacheProvider;
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
        /// <returns></returns>
        protected override IEnumerable<TResult> ExecuteReaderImpl<TResult>(Action<DbDataReader, List<TResult>> getObjectDel,
            string storedProcedure, int? commandTimeout, string[] partitionOnArr, bool validateSelectColumns, DbConnection userConn)
        {
            try
            {
                // Try open connection if not already open.
                if (userConn == null)
                    OpenConn(_conn);
                else
                {
                    _conn = userConn as SqlConnection;
                }

                List<TResult> result = new List<TResult>();
                using (SqlCommand command = new SqlCommand(storedProcedure, _conn))
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
                
                return result;
            }

            finally
            {
                if (userConn == null)
                    _conn.Close();
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
        /// <returns></returns>
        protected override async Task<IEnumerable<TResult>> ExecuteReaderAsyncImpl<TResult>(Action<DbDataReader, List<TResult>> getObjectDel,
            string storedProcedure, int? commandTimeout, string[] partitionOnArr, bool validateSelectColumns, DbConnection userConn)
        {
            try
            {
                // Try open connection if not already open.
                if (userConn == null)
                    await OpenConnAsync(_conn);
                else
                {
                    _conn = userConn as SqlConnection;
                }

                List<TResult> result = new List<TResult>();

                using (SqlCommand command = new SqlCommand(storedProcedure, _conn))
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
                return result;
            }

            finally
            {
                if (userConn == null)
                    _conn.Close();
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
                    OpenConn(_conn);
                else
                {
                    _conn = userConn as SqlConnection;
                }

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
                    _conn.Close();
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
                    await OpenConnAsync(_conn);
                else
                {
                    _conn = userConn as SqlConnection;
                }

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
                    _conn.Close();
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
                    OpenConn(_conn);
                else
                {
                    _conn = userConn as SqlConnection;
                }

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
                    _conn.Close();
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
                    await OpenConnAsync(_conn);
                else
                {
                    _conn = userConn as SqlConnection;
                }
              
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
                    _conn.Close();
            }

        }
    }
}
