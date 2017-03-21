using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SprocMapperLibrary.SqlServer
{
    /// <summary>
    /// 
    /// </summary>
    public class SqlServerProcedure : BaseProcedure
    {
        private readonly SqlConnection _conn;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        public SqlServerProcedure(SqlConnection conn) : base()
        {
            _conn = conn;
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
