using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace SprocMapperLibrary
{
    public class Select12<T> : AbstractSelect
    {
        public Select12(List<ISprocObjectMap> sprocObjectMapList) : base(sprocObjectMapList){}

        public Select12<T> AddSqlParameter(SqlParameter item)
        {
            ParamList.Add(item);
            return this;
        }

        public Select12<T> AddSqlParameter(string parameterName, SqlDbType dbType, object value)
        {
            ParamList.Add(new SqlParameter(parameterName, dbType) { Value = value });
            return this;
        }

        public IEnumerable<T> ExecuteReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(SqlConnection conn, string procName, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T> callBack,
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
                Dictionary<string, PropertyInfo> obj4PropertyCache = new Dictionary<string, PropertyInfo>();
                Dictionary<string, PropertyInfo> obj5PropertyCache = new Dictionary<string, PropertyInfo>();
                Dictionary<string, PropertyInfo> obj6PropertyCache = new Dictionary<string, PropertyInfo>();
                Dictionary<string, PropertyInfo> obj7PropertyCache = new Dictionary<string, PropertyInfo>();
                Dictionary<string, PropertyInfo> obj8PropertyCache = new Dictionary<string, PropertyInfo>();
                Dictionary<string, PropertyInfo> obj9PropertyCache = new Dictionary<string, PropertyInfo>();
                Dictionary<string, PropertyInfo> obj10PropertyCache = new Dictionary<string, PropertyInfo>();
                Dictionary<string, PropertyInfo> obj11PropertyCache = new Dictionary<string, PropertyInfo>();
                Dictionary<string, PropertyInfo> obj12PropertyCache = new Dictionary<string, PropertyInfo>();

                while (reader.Read())
                {
                    T1 obj1 = SprocMapperHelper.GetObject<T1>(SprocObjectMapList[0].Columns,
                        SprocObjectMapList[0].CustomColumnMappings, reader, objPropertyCache);
                    T2 obj2 = SprocMapperHelper.GetObject<T2>(SprocObjectMapList[1].Columns,
                        SprocObjectMapList[1].CustomColumnMappings, reader, obj2PropertyCache);
                    T3 obj3 = SprocMapperHelper.GetObject<T3>(SprocObjectMapList[2].Columns,
                        SprocObjectMapList[2].CustomColumnMappings, reader, obj3PropertyCache);
                    T4 obj4 = SprocMapperHelper.GetObject<T4>(SprocObjectMapList[3].Columns,
                        SprocObjectMapList[3].CustomColumnMappings, reader, obj4PropertyCache);
                    T5 obj5 = SprocMapperHelper.GetObject<T5>(SprocObjectMapList[4].Columns,
                        SprocObjectMapList[4].CustomColumnMappings, reader, obj5PropertyCache);
                    T6 obj6 = SprocMapperHelper.GetObject<T6>(SprocObjectMapList[5].Columns,
                         SprocObjectMapList[5].CustomColumnMappings, reader, obj6PropertyCache);
                    T7 obj7 = SprocMapperHelper.GetObject<T7>(SprocObjectMapList[6].Columns,
                        SprocObjectMapList[6].CustomColumnMappings, reader, obj7PropertyCache);
                    T8 obj8 = SprocMapperHelper.GetObject<T8>(SprocObjectMapList[7].Columns,
                        SprocObjectMapList[7].CustomColumnMappings, reader, obj8PropertyCache);
                    T9 obj9 = SprocMapperHelper.GetObject<T9>(SprocObjectMapList[8].Columns,
                        SprocObjectMapList[8].CustomColumnMappings, reader, obj9PropertyCache);
                    T10 obj10 = SprocMapperHelper.GetObject<T10>(SprocObjectMapList[9].Columns,
                        SprocObjectMapList[9].CustomColumnMappings, reader, obj10PropertyCache);
                    T11 obj11 = SprocMapperHelper.GetObject<T11>(SprocObjectMapList[10].Columns,
                        SprocObjectMapList[10].CustomColumnMappings, reader, obj11PropertyCache);
                    T12 obj12 = SprocMapperHelper.GetObject<T12>(SprocObjectMapList[11].Columns,
                        SprocObjectMapList[11].CustomColumnMappings, reader, obj12PropertyCache);

                    T obj = callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8, obj9, obj10, obj11, obj12);

                    result.Add(obj);
                }

            }
            return result;
        }
    }

}

