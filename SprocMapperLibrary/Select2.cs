using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
namespace SprocMapperLibrary
{
    public class Select2<T> : AbstractSelect
    {
        public Select2(List<ISprocObjectMap> sprocObjectMapList) : base(sprocObjectMapList){}

        public Select2<T> AddSqlParameter(SqlParameter item)
        {
            ParamList.Add(item);
            return this;
        }

        public Select2<T> AddSqlParameter(string parameterName, SqlDbType dbType, object value)
        {
            ParamList.Add(new SqlParameter(parameterName, dbType) { Value = value });
            return this;
        }


        public List<T> ExecuteReader<T1, T2>(SqlConnection conn, string procName, Func<T1, T2, T> callBack,
            int commandTimeout = 600)
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

                while (reader.Read())
                {
                    T1 obj1 = SprocMapperHelper.GetObject<T1>(SprocObjectMapList[0].Columns,
                        SprocObjectMapList[0].CustomColumnMappings, reader);
                    T2 obj2 = SprocMapperHelper.GetObject<T2>(SprocObjectMapList[1].Columns,
                        SprocObjectMapList[1].CustomColumnMappings, reader);

                    T obj = callBack.Invoke(obj1, obj2);

                    result.Add(obj);
                }

            }
            return result;
        }
    }

}

