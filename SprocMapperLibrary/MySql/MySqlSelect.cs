using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FastMember;
using MySql.Data.MySqlClient;
using SprocMapperLibrary.Interface;

namespace SprocMapperLibrary.MySql
{
    /// <summary>
    /// 
    /// </summary>
    public class MySqlSelect : BaseSelect
    {
        private readonly MySqlConnection _conn;
        /// <summary>
        /// 
        /// </summary>
        public MySqlSelect(MySqlConnection conn) : base()
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
            using (MySqlCommand command = new MySqlCommand(storedProcedure, _conn))
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

            using (MySqlCommand command = new MySqlCommand(storedProcedure, _conn))
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
    }
}