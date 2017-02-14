using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SprocMapperLibrary
{
    public class Select : AbstractSelect
    {
        private readonly Dictionary<Type, ISprocObjectMap> sprocObjectMapDic;
        public Select() : base()
        {
            sprocObjectMapDic = new Dictionary<Type, ISprocObjectMap>();
        }

        public Select AddSqlParameter(SqlParameter item)
        {
            ParamList.Add(item);
            return this;
        }

        public Select AddSqlParameter(string parameterName, SqlDbType dbType, object value)
        {
            ParamList.Add(new SqlParameter(parameterName, dbType) {Value = value});
            return this;
        }

        public IEnumerable<T> ExecuteReader<T>(SqlConnection conn, string procName, MapObject<T> objectMap = null, int commandTimeout = 600)
        {
            SprocObjectMapList = new List<ISprocObjectMap>();

            MapObject<T>();

            OpenConn(conn);

            List<T> result = new List<T>();

            using (SqlCommand command = new SqlCommand(procName, conn))
            {
                SetCommandProps(command, commandTimeout);

                using (var reader = command.ExecuteReader())
                {
                    DataTable schema = reader.GetSchemaTable();
                   
                    ValidateSchema(schema);
                    DoStrictValidation(schema);

                    if (!reader.HasRows)
                    {
                        return (List<T>)Activator.CreateInstance(typeof(List<T>));
                    }

                    while (reader.Read())
                    {
                        T obj = SprocMapperHelper.GetObject<T>(SprocObjectMapList[0], reader);

                        result.Add(obj);
                    }
                }
            }
            return result;

        }

        public IEnumerable<T> ExecuteReader<T, T1>(SqlConnection conn, string procName, Action<T, T1> callBack,
            int commandTimeout = 600)
        {
            SprocObjectMapList = new List<ISprocObjectMap>();

            MapObject<T>();
            MapObject<T1>();

            OpenConn(conn);

            List<T> result = new List<T>();
            using (SqlCommand command = new SqlCommand(procName, conn))
            {
                SetCommandProps(command, commandTimeout);

                using (var reader = command.ExecuteReader())
                {
                    DataTable schema = reader.GetSchemaTable();

                    SetOrdinal(schema, SprocObjectMapList);
                    ValidateSchema(schema);
                    DoStrictValidation(schema);

                    if (!reader.HasRows)
                        return (List<T>)Activator.CreateInstance(typeof(List<T>));

                    while (reader.Read())
                    {
                        T obj1 = SprocMapperHelper.GetObject<T>(SprocObjectMapList[0], reader);
                        T1 obj2 = SprocMapperHelper.GetObject<T1>(SprocObjectMapList[1], reader);

                        callBack.Invoke(obj1, obj2);

                        result.Add(obj1);
                    }
                }
            }
            return result;
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2>(SqlConnection conn, string procName, Action<T, T1, T2> callBack, int commandTimeout = 600)
        {
            SprocObjectMapList = new List<ISprocObjectMap>();

            MapObject<T>();
            MapObject<T1>();
            MapObject<T2>();

            OpenConn(conn);

            List<T> result = new List<T>();
            using (SqlCommand command = new SqlCommand(procName, conn))
            {
                SetCommandProps(command, commandTimeout);

                using (var reader = command.ExecuteReader())
                {
                    DataTable schema = reader.GetSchemaTable();

                    ValidateSchema(schema);
                    DoStrictValidation(schema);

                    if (!reader.HasRows)
                        return (List<T>)Activator.CreateInstance(typeof(List<T>));

                    while (reader.Read())
                    {
                        T obj1 = SprocMapperHelper.GetObject<T>(SprocObjectMapList[0], reader);
                        T1 obj2 = SprocMapperHelper.GetObject<T1>(SprocObjectMapList[1], reader);
                        T2 obj3 = SprocMapperHelper.GetObject<T2>(SprocObjectMapList[2], reader);

                        callBack.Invoke(obj1, obj2, obj3);

                        result.Add(obj1);
                    }
                }

            }
            return result;
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3>(SqlConnection conn, string procName, Action<T, T1, T2, T3> callBack, int commandTimeout = 600)
        {
            SprocObjectMapList = new List<ISprocObjectMap>();

            MapObject<T>();
            MapObject<T1>();
            MapObject<T2>();
            MapObject<T3>();

            OpenConn(conn);

            List<T> result = new List<T>();
            using (SqlCommand command = new SqlCommand(procName, conn))
            {
                SetCommandProps(command, commandTimeout);

                using (var reader = command.ExecuteReader())
                {
                    DataTable schema = reader.GetSchemaTable();

                    ValidateSchema(schema);
                    DoStrictValidation(schema);

                    if (!reader.HasRows)
                        return (List<T>)Activator.CreateInstance(typeof(List<T>));

                    while (reader.Read())
                    {
                        T obj1 = SprocMapperHelper.GetObject<T>(SprocObjectMapList[0], reader);
                        T1 obj2 = SprocMapperHelper.GetObject<T1>(SprocObjectMapList[1], reader);
                        T2 obj3 = SprocMapperHelper.GetObject<T2>(SprocObjectMapList[2], reader);
                        T3 obj4 = SprocMapperHelper.GetObject<T3>(SprocObjectMapList[3], reader);

                        callBack.Invoke(obj1, obj2, obj3, obj4);

                        result.Add(obj1);
                    }
                }
            }
            return result;
        }


        public IEnumerable<T> ExecuteReader<T, T1, T2, T3, T4>(SqlConnection conn, string procName, Action<T, T1, T2, T3, T4> callBack, int commandTimeout = 600)
        {
            SprocObjectMapList = new List<ISprocObjectMap>();

            MapObject<T>();
            MapObject<T1>();
            MapObject<T2>();
            MapObject<T3>();
            MapObject<T4>();

            OpenConn(conn);

            List<T> result = new List<T>();
            using (SqlCommand command = new SqlCommand(procName, conn))
            {
                SetCommandProps(command, commandTimeout);

                using (var reader = command.ExecuteReader())
                {
                    DataTable schema = reader.GetSchemaTable();

                    ValidateSchema(schema);
                    DoStrictValidation(schema);

                    if (!reader.HasRows)
                        return (List<T>)Activator.CreateInstance(typeof(List<T>));

                    while (reader.Read())
                    {
                        T obj1 = SprocMapperHelper.GetObject<T>(SprocObjectMapList[0], reader);
                        T1 obj2 = SprocMapperHelper.GetObject<T1>(SprocObjectMapList[1], reader);
                        T2 obj3 = SprocMapperHelper.GetObject<T2>(SprocObjectMapList[2], reader);
                        T3 obj4 = SprocMapperHelper.GetObject<T3>(SprocObjectMapList[3], reader);
                        T4 obj5 = SprocMapperHelper.GetObject<T4>(SprocObjectMapList[4], reader);

                        callBack.Invoke(obj1, obj2, obj3, obj4, obj5);

                        result.Add(obj1);
                    }
                }

            }
            return result;
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3, T4, T5>(SqlConnection conn, string procName, Action<T, T1, T2, T3, T4, T5> callBack, int commandTimeout = 600)
        {
            SprocObjectMapList = new List<ISprocObjectMap>();

            MapObject<T>();
            MapObject<T1>();
            MapObject<T2>();
            MapObject<T3>();
            MapObject<T4>();
            MapObject<T5>();

            OpenConn(conn);

            List<T> result = new List<T>();
            using (SqlCommand command = new SqlCommand(procName, conn))
            {
                SetCommandProps(command, commandTimeout);

                using (var reader = command.ExecuteReader())
                {
                    DataTable schema = reader.GetSchemaTable();

                    ValidateSchema(schema);
                    DoStrictValidation(schema);

                    if (!reader.HasRows)
                        return (List<T>)Activator.CreateInstance(typeof(List<T>));

                    while (reader.Read())
                    {
                        T obj1 = SprocMapperHelper.GetObject<T>(SprocObjectMapList[0], reader);
                        T1 obj2 = SprocMapperHelper.GetObject<T1>(SprocObjectMapList[1], reader);
                        T2 obj3 = SprocMapperHelper.GetObject<T2>(SprocObjectMapList[2], reader);
                        T3 obj4 = SprocMapperHelper.GetObject<T3>(SprocObjectMapList[3], reader);
                        T4 obj5 = SprocMapperHelper.GetObject<T4>(SprocObjectMapList[4], reader);
                        T5 obj6 = SprocMapperHelper.GetObject<T5>(SprocObjectMapList[5], reader);

                        callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6);

                        result.Add(obj1);
                    }
                }
            }
            return result;
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3, T4, T5, T6>(SqlConnection conn, string procName, Action<T, T1, T2, T3, T4, T5, T6> callBack, int commandTimeout = 600)
        {
            SprocObjectMapList = new List<ISprocObjectMap>();

            MapObject<T>();
            MapObject<T1>();
            MapObject<T2>();
            MapObject<T3>();
            MapObject<T4>();
            MapObject<T5>();
            MapObject<T6>();

            OpenConn(conn);

            List<T> result = new List<T>();
            using (SqlCommand command = new SqlCommand(procName, conn))
            {
                SetCommandProps(command, commandTimeout);

                using (var reader = command.ExecuteReader())
                {
                    DataTable schema = reader.GetSchemaTable();

                    ValidateSchema(schema);
                    DoStrictValidation(schema);

                    if (!reader.HasRows)
                        return (List<T>)Activator.CreateInstance(typeof(List<T>));

                    while (reader.Read())
                    {
                        T obj1 = SprocMapperHelper.GetObject<T>(SprocObjectMapList[0], reader);
                        T1 obj2 = SprocMapperHelper.GetObject<T1>(SprocObjectMapList[1], reader);
                        T2 obj3 = SprocMapperHelper.GetObject<T2>(SprocObjectMapList[2], reader);
                        T3 obj4 = SprocMapperHelper.GetObject<T3>(SprocObjectMapList[3], reader);
                        T4 obj5 = SprocMapperHelper.GetObject<T4>(SprocObjectMapList[4], reader);
                        T5 obj6 = SprocMapperHelper.GetObject<T5>(SprocObjectMapList[5], reader);
                        T6 obj7 = SprocMapperHelper.GetObject<T6>(SprocObjectMapList[6], reader);

                        callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7);

                        result.Add(obj1);
                    }
                }
            }
            return result;
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3, T4, T5, T6, T7>(SqlConnection conn, string procName, Action<T, T1, T2, T3, T4, T5, T6, T7> callBack, int commandTimeout = 600)
        {
            SprocObjectMapList = new List<ISprocObjectMap>();

            MapObject<T>();
            MapObject<T1>();
            MapObject<T2>();
            MapObject<T3>();
            MapObject<T4>();
            MapObject<T5>();
            MapObject<T6>();
            MapObject<T7>();

            OpenConn(conn);

            List<T> result = new List<T>();
            using (SqlCommand command = new SqlCommand(procName, conn))
            {
                SetCommandProps(command, commandTimeout);

                using (var reader = command.ExecuteReader())
                {
                    DataTable schema = reader.GetSchemaTable();

                    ValidateSchema(schema);
                    DoStrictValidation(schema);

                    if (!reader.HasRows)
                        return (List<T>)Activator.CreateInstance(typeof(List<T>));

                    while (reader.Read())
                    {
                        T obj1 = SprocMapperHelper.GetObject<T>(SprocObjectMapList[0], reader);
                        T1 obj2 = SprocMapperHelper.GetObject<T1>(SprocObjectMapList[1], reader);
                        T2 obj3 = SprocMapperHelper.GetObject<T2>(SprocObjectMapList[2], reader);
                        T3 obj4 = SprocMapperHelper.GetObject<T3>(SprocObjectMapList[3], reader);
                        T4 obj5 = SprocMapperHelper.GetObject<T4>(SprocObjectMapList[4], reader);
                        T5 obj6 = SprocMapperHelper.GetObject<T5>(SprocObjectMapList[5], reader);
                        T6 obj7 = SprocMapperHelper.GetObject<T6>(SprocObjectMapList[6], reader);
                        T7 obj8 = SprocMapperHelper.GetObject<T7>(SprocObjectMapList[7], reader);

                        callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8);

                        result.Add(obj1);
                    }
                }

            }
            return result;
        }

        public Select AddMapping<T>(MapObject<T> propertyMap)
        {
            if (propertyMap == null)
                throw new NullReferenceException(nameof(propertyMap));

            if (!sprocObjectMapDic.ContainsKey(typeof(T)))
            {
                propertyMap.AddAllColumns();
                sprocObjectMapDic.Add(typeof(T), propertyMap.GetMap());
            }

            return this;
        }

        private void MapObject<T>()
        {
            ISprocObjectMap objMap;
            if (!sprocObjectMapDic.TryGetValue(typeof(T), out objMap))
            {
                var map = PropertyMapper.MapObject<T>();
                map.AddAllColumns();

                objMap = map.GetMap();
            }

            SprocObjectMapList.Add(objMap);
        }
    }
}

