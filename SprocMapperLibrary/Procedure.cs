using System.Data;
using System.Data.SqlClient;

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
            using (SqlCommand command = new SqlCommand(storedProcedure, conn))
            {
                SetCommandProps(command, commandTimeout);
                affectedRecords = command.ExecuteNonQuery();                
            }

            return affectedRecords;
        }

        public T ExecuteScalar<T>(SqlConnection conn, string storedProcedure, int commandTimeout = 600)
        {
            T obj = default(T);

            using (SqlCommand command = new SqlCommand(storedProcedure, conn))
            {
                SetCommandProps(command, commandTimeout);
                obj = (T)command.ExecuteScalar();
            }

            return obj;
        }
    }
}
