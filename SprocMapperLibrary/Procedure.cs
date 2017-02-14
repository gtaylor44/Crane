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

        public Procedure AddSqlParameter(string parameterName, SqlDbType dbType, object value)
        {
            ParamList.Add(new SqlParameter(parameterName, dbType) { Value = value });
            return this;
        }

        public int ExecuteNonQuery(SqlConnection conn, string storedProcedure, int commandTimeout = 600)
        {
            int affectedRecords;

            conn.Open();
            using (SqlCommand command = new SqlCommand(storedProcedure, conn))
            {
                SetCommandProps(command, commandTimeout);
                affectedRecords = command.ExecuteNonQuery();                
            }

            return affectedRecords;
        }

        public async Task<int> ExecuteNonQueryAsync(SqlConnection conn, string storedProcedure, int commandTimeout = 600)
        {
            int affectedRecords;

            await conn.OpenAsync();
            using (SqlCommand command = new SqlCommand(storedProcedure, conn))
            {
                SetCommandProps(command, commandTimeout);
                affectedRecords = await command.ExecuteNonQueryAsync();
            }

            return affectedRecords;
        }

        public T ExecuteScalar<T>(SqlConnection conn, string storedProcedure, int commandTimeout = 600)
        {
            T obj = default(T);

            conn.Open();
            using (SqlCommand command = new SqlCommand(storedProcedure, conn))
            {
                SetCommandProps(command, commandTimeout);
                obj = (T)command.ExecuteScalar();
            }

            return obj;
        }

        public async Task<T> ExecuteScalarAsync<T>(SqlConnection conn, string storedProcedure, int commandTimeout = 600)
        {
            T obj = default(T);

            await conn.OpenAsync();
            using (SqlCommand command = new SqlCommand(storedProcedure, conn))
            {
                SetCommandProps(command, commandTimeout);
                obj = (T)await command.ExecuteScalarAsync();
            }

            return obj;
        }
    }
}
