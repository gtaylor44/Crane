using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SprocMapperLibrary
{
    public class Procedure : AbstractQuery
    {
        public Procedure AddSqlParameter(SqlParameter item)
        {
            ParamList.Add(item);
            return this;
        }

        public Procedure AddSqlParameter(string parameterName, object value)
        {
            ParamList.Add(new SqlParameter() { Value = value, ParameterName = parameterName });
            return this;
        }

        public Procedure AddSqlParameter(string parameterName, object value, SqlDbType dbType)
        {
            ParamList.Add(new SqlParameter() { Value = value, ParameterName = parameterName, SqlDbType = dbType});
            return this;
        }

        /// <summary>
        /// Execute a stored procedure synchronously.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>Number of affected records.</returns>
        public int ExecuteNonQuery(SqlConnection conn, string storedProcedure, int? commandTimeout = null)
        {
            int affectedRecords;

            OpenConn(conn);

            using (SqlCommand command = new SqlCommand(storedProcedure, conn))
            {
                SetCommandProps(command, commandTimeout);
                affectedRecords = command.ExecuteNonQuery();                
            }

            return affectedRecords;
        }

        /// <summary>
        /// Execute a stored procedure asynchronously.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>Number of affected records.</returns>
        public async Task<int> ExecuteNonQueryAsync(SqlConnection conn, string storedProcedure, int? commandTimeout = null)
        {
            int affectedRecords;

            await OpenConnAsync(conn);
            using (SqlCommand command = new SqlCommand(storedProcedure, conn))
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
        /// <param name="conn"></param>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>First column of the first row in the result set.</returns>
        public T ExecuteScalar<T>(SqlConnection conn, string storedProcedure, int? commandTimeout = null)
        {
            T obj;

            OpenConn(conn);
            using (SqlCommand command = new SqlCommand(storedProcedure, conn))
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
        /// <param name="conn"></param>
        /// <param name="storedProcedure"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>First column of the first row in the result set.</returns>
        public async Task<T> ExecuteScalarAsync<T>(SqlConnection conn, string storedProcedure, int? commandTimeout = null)
        {
            T obj;

            await OpenConnAsync(conn);
            using (SqlCommand command = new SqlCommand(storedProcedure, conn))
            {
                SetCommandProps(command, commandTimeout);
                obj = (T)await command.ExecuteScalarAsync();
            }

            return obj;
        }
    }
}
