using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace SprocMapperLibrary
{
    public class Select8<T> : AbstractSelect
    {
        public Select8(List<ISprocObjectMap> sprocObjectMapList) : base(sprocObjectMapList){}

        public Select8<T> AddSqlParameter(SqlParameter item)
        {
            ParamList.Add(item);
            return this;
        }

        public Select8<T> AddSqlParameter(string parameterName, SqlDbType dbType, object value)
        {
            ParamList.Add(new SqlParameter(parameterName, dbType) { Value = value });
            return this;
        }

        public IEnumerable<T> ExecuteReader<T1, T2, T3, T4, T5, T6, T7, T8>(SqlConnection conn, string procName, Func<T1, T2, T3, T4, T5, T6, T7, T8, T> callBack,
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
                    T1 obj1 = SprocMapperHelper.GetObject<T1>(SprocObjectMapList[0], reader);
                    T2 obj2 = SprocMapperHelper.GetObject<T2>(SprocObjectMapList[1], reader);
                    T3 obj3 = SprocMapperHelper.GetObject<T3>(SprocObjectMapList[2], reader);
                    T4 obj4 = SprocMapperHelper.GetObject<T4>(SprocObjectMapList[3], reader);
                    T5 obj5 = SprocMapperHelper.GetObject<T5>(SprocObjectMapList[4], reader);
                    T6 obj6 = SprocMapperHelper.GetObject<T6>(SprocObjectMapList[5], reader);
                    T7 obj7 = SprocMapperHelper.GetObject<T7>(SprocObjectMapList[6], reader);
                    T8 obj8 = SprocMapperHelper.GetObject<T8>(SprocObjectMapList[7], reader);

                    T obj = callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8);

                    result.Add(obj);
                }

            }
            return result;
        }
    }

}

