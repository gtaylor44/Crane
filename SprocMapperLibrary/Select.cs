using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace SprocMapperLibrary
{
    public class Select<T> : AbstractSelect
    {
        public Select(List<ISprocObjectMap> sprocObjectMapList) : base(sprocObjectMapList){ }

        public Select<T> AddSqlParameter(SqlParameter item)
        {
            ParamList.Add(item);
            return this;
        }

        public Select<T> AddSqlParameter(string parameterName, SqlDbType dbType, object value)
        {
            ParamList.Add(new SqlParameter(parameterName, dbType) {Value = value});
            return this;
        }

        public IEnumerable<T> ExecuteReader(SqlConnection conn, string procName, int commandTimeout = 600)
        {
            ValidateProperties();
            OpenConn(conn);

            List<T> result = new List<T>();

            using (SqlCommand command = new SqlCommand(procName, conn))
            {
                SetCommandProps(command, commandTimeout);

                var reader = command.ExecuteReader();

                DataTable schema = reader.GetSchemaTable();

                ValidateSchema(schema);
                RemoveAbsentColumns(schema);

                if (!reader.HasRows)
                    return default(List<T>);

                Dictionary<string, PropertyInfo> objPropertyCache = new Dictionary<string, PropertyInfo>();

                while (reader.Read())
                {
                    T obj = SprocMapperHelper.GetObject<T>(SprocObjectMapList[0].Columns,
                        SprocObjectMapList[0].CustomColumnMappings, reader, objPropertyCache);
                    result.Add(obj);
                }

            }
            return result;

        }
    }

}

