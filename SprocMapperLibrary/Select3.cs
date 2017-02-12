using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace SprocMapperLibrary
{
    public class Select3<T> : AbstractSelect
    {
        public Select3(List<ISprocObjectMap> sprocObjectMapList) : base(sprocObjectMapList){}

        public Select3<T> AddSqlParameter(SqlParameter item)
        {
            ParamList.Add(item);
            return this;
        }

        public Select3<T> AddSqlParameter(string parameterName, SqlDbType dbType, object value)
        {
            ParamList.Add(new SqlParameter(parameterName, dbType) { Value = value });
            return this;
        }

        public IEnumerable<T> ExecuteReader<T1, T2, T3>(SqlConnection conn, string procName, Func<T1, T2, T3, T> callBack,
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

                Dictionary<string, PropertyInfo> objPropertyCache = new Dictionary<string, PropertyInfo>();
                Dictionary<string, PropertyInfo> obj2PropertyCache = new Dictionary<string, PropertyInfo>();
                Dictionary<string, PropertyInfo> obj3PropertyCache = new Dictionary<string, PropertyInfo>();

                while (reader.Read())
                {
                    T1 obj1 = SprocMapperHelper.GetObject<T1>(SprocObjectMapList[0].Columns,
                        SprocObjectMapList[0].CustomColumnMappings, reader, objPropertyCache);
                    T2 obj2 = SprocMapperHelper.GetObject<T2>(SprocObjectMapList[1].Columns,
                        SprocObjectMapList[1].CustomColumnMappings, reader, obj2PropertyCache);
                    T3 obj3 = SprocMapperHelper.GetObject<T3>(SprocObjectMapList[2].Columns,
                        SprocObjectMapList[2].CustomColumnMappings, reader, obj3PropertyCache);

                    T obj = callBack.Invoke(obj1, obj2, obj3);

                    result.Add(obj);
                }

            }
            return result;
        }
    }

}

