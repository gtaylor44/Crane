using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SprocMapperLibrary.SqlServer
{
    /// <summary>
    /// 
    /// </summary>
    public class SqlServerSproc : BaseSproc
    {
        private readonly SqlConnection _conn;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        public SqlServerSproc(SqlConnection conn) : base()
        {
            _conn = conn;
        }

        /// <summary>
        /// Performs synchronous version of stored procedure.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="getObjectDel"></param>
        /// <param name="storedProcedure">The name of your stored procedure (with schema name if applicable).</param>
        /// <param name="commandTimeout"></param>
        /// <param name="partitionOn"></param>
        /// <param name="validatePartitionOn"></param>
        /// <param name="validateSelectColumns"></param>
        /// <returns></returns>
        protected override IEnumerable<TResult> ExecuteReaderImpl<TResult>(Action<DbDataReader, List<TResult>> getObjectDel,
            string storedProcedure, int? commandTimeout, string partitionOn, bool validatePartitionOn,
            bool validateSelectColumns)
        {
            if (validatePartitionOn)
                SprocMapper.ValidatePartitionOn(partitionOn);

            // Try open connection if not already open.
            OpenConn(_conn);

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

                    if (partitionOn != null)
                        partitionOnOrdinal = SprocMapper.GetOrdinalPartition(rowList, partitionOn, SprocObjectMapList.Count);

                    SprocMapper.SetOrdinal(rowList, SprocObjectMapList, partitionOnOrdinal);

                    if (validateSelectColumns)
                        SprocMapper.ValidateSelectColumns(rowList, SprocObjectMapList, partitionOnOrdinal, storedProcedure);

                    SprocMapper.ValidateSchema(schema, SprocObjectMapList);

                    if (!reader.HasRows)
                        return (List<TResult>)Activator.CreateInstance(typeof(List<TResult>));

                    while (reader.Read())
                    {
                        getObjectDel(reader, result);
                    }
                }

            }
            return result;
        }

        /// <summary>
        /// Performs asynchronous version of stored procedure.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="getObjectDel"></param>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="partitionOn"></param>
        /// <param name="validatePartitionOn"></param>
        /// <param name="validateSelectColumns"></param>
        /// <returns></returns>
        protected override async Task<IEnumerable<TResult>> ExecuteReaderAsyncImpl<TResult>(Action<DbDataReader, List<TResult>> getObjectDel,
            string storedProcedure, int? commandTimeout, string partitionOn,
            bool validatePartitionOn, bool validateSelectColumns)
        {
            if (validatePartitionOn)
                SprocMapper.ValidatePartitionOn(partitionOn);

            // Try open connection if not already open.
            await OpenConnAsync(_conn);

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

                    if (partitionOn != null)
                        partitionOnOrdinal = SprocMapper.GetOrdinalPartition(rowList, partitionOn, SprocObjectMapList.Count);

                    SprocMapper.SetOrdinal(rowList, SprocObjectMapList, partitionOnOrdinal);

                    if (validateSelectColumns)
                        SprocMapper.ValidateSelectColumns(rowList, SprocObjectMapList, partitionOnOrdinal, storedProcedure);

                    SprocMapper.ValidateSchema(schema, SprocObjectMapList);

                    if (!reader.HasRows)
                        return (List<TResult>)Activator.CreateInstance(typeof(List<TResult>));

                    while (reader.Read())
                    {
                        getObjectDel(reader, result);
                    }
                }

            }
            return result;
        }

        /// <summary>
        /// Execute a MSSql stored procedure synchronously.
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>Number of affected records.</returns>
        public override int ExecuteNonQuery(string storedProcedure, int? commandTimeout = null)
        {
            int affectedRecords;

            OpenConn(_conn);

            using (SqlCommand command = new SqlCommand(storedProcedure, _conn))
            {
                SetCommandProps(command, commandTimeout);
                affectedRecords = command.ExecuteNonQuery();                
            }

            return affectedRecords;
        }

        /// <summary>
        /// Execute a stored procedure asynchronously.
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>Number of affected records.</returns>
        public override async Task<int> ExecuteNonQueryAsync(string storedProcedure, int? commandTimeout = null)
        {
            int affectedRecords;

            await OpenConnAsync(_conn);
            using (SqlCommand command = new SqlCommand(storedProcedure, _conn))
            {
                SetCommandProps(command, commandTimeout);
                affectedRecords = await command.ExecuteNonQueryAsync();
            }

            return affectedRecords;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>First column of the first row in the result set.</returns>
        public override T ExecuteScalar<T>(string storedProcedure, int? commandTimeout = null)
        {
            T obj;

            OpenConn(_conn);
            using (SqlCommand command = new SqlCommand(storedProcedure, _conn))
            {
                SetCommandProps(command, commandTimeout);
                obj = (T)command.ExecuteScalar();
            }

            return obj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>First column of the first row in the result set.</returns>
        public override async Task<T> ExecuteScalarAsync<T>(string storedProcedure, int? commandTimeout = null)
        {
            T obj;

            await OpenConnAsync(_conn);
            using (SqlCommand command = new SqlCommand(storedProcedure, _conn))
            {
                SetCommandProps(command, commandTimeout);
                obj = (T)await command.ExecuteScalarAsync();
            }

            return obj;
        }
    }
}
