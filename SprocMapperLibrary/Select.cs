using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SprocMapperLibrary
{
    public class Select : AbstractSelect
    {
        public Select() : base()
        {
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
            MapObject(objectMap, SprocObjectMapList);

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
                {
                    return (List<T>) Activator.CreateInstance(typeof(List<T>));
                }
                    
                while (reader.Read())
                {
                    T obj = SprocMapperHelper.GetObject<T>(SprocObjectMapList[0], reader);

                    result.Add(obj);
                }

            }
            return result;

        }

        public IEnumerable<T> ExecuteReader<T, T1>(SqlConnection conn, string procName, Action<T, T1> callBack,
            MapObject<T> objectMap = null, MapObject<T1> objectMap2 = null,
            int commandTimeout = 600)
        {
            SprocObjectMapList = new List<ISprocObjectMap>();

            MapObject(objectMap, SprocObjectMapList);
            MapObject(objectMap2, SprocObjectMapList);

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
                    return (List<T>)Activator.CreateInstance(typeof(List<T>));

                while (reader.Read())
                {
                    T obj1 = SprocMapperHelper.GetObject<T>(SprocObjectMapList[0], reader);
                    T1 obj2 = SprocMapperHelper.GetObject<T1>(SprocObjectMapList[1], reader);

                    callBack.Invoke(obj1, obj2);

                    result.Add(obj1);
                }

            }
            return result;
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2>(SqlConnection conn, string procName, Action<T, T1, T2> callBack,
            MapObject<T> objectMap = null, MapObject<T1> objectMap2 = null, MapObject<T2> objectMap3 = null,
            int commandTimeout = 600)
        {
            SprocObjectMapList = new List<ISprocObjectMap>();

            MapObject(objectMap, SprocObjectMapList);
            MapObject(objectMap2, SprocObjectMapList);
            MapObject(objectMap3, SprocObjectMapList);

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
            return result;
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3>(SqlConnection conn, string procName, Action<T, T1, T2, T3> callBack,
            MapObject<T> objectMap = null, MapObject<T1> objectMap2 = null, MapObject<T2> objectMap3 = null, MapObject<T3> objectMap4 = null,
            int commandTimeout = 600)
        {
            SprocObjectMapList = new List<ISprocObjectMap>();

            MapObject(objectMap, SprocObjectMapList);
            MapObject(objectMap2, SprocObjectMapList);
            MapObject(objectMap3, SprocObjectMapList);
            MapObject(objectMap4, SprocObjectMapList);

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
            return result;
        }


        public IEnumerable<T> ExecuteReader<T, T1, T2, T3, T4>(SqlConnection conn, string procName, Action<T, T1, T2, T3, T4> callBack,
    MapObject<T> objectMap = null, MapObject<T1> objectMap2 = null, MapObject<T2> objectMap3 = null, MapObject<T3> objectMap4 = null, MapObject<T4> objectMap5 = null,
    int commandTimeout = 600)
        {
            SprocObjectMapList = new List<ISprocObjectMap>();

            MapObject(objectMap, SprocObjectMapList);
            MapObject(objectMap2, SprocObjectMapList);
            MapObject(objectMap3, SprocObjectMapList);
            MapObject(objectMap4, SprocObjectMapList);
            MapObject(objectMap5, SprocObjectMapList);

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
            return result;
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3, T4, T5>(SqlConnection conn, string procName, Action<T, T1, T2, T3, T4, T5> callBack,
            MapObject<T> objectMap = null, MapObject<T1> objectMap2 = null, MapObject<T2> objectMap3 = null, MapObject<T3> objectMap4 = null, MapObject<T4> objectMap5 = null, MapObject<T5> objectMap6 = null,
            int commandTimeout = 600)
        {
            SprocObjectMapList = new List<ISprocObjectMap>();

            MapObject(objectMap, SprocObjectMapList);
            MapObject(objectMap2, SprocObjectMapList);
            MapObject(objectMap3, SprocObjectMapList);
            MapObject(objectMap4, SprocObjectMapList);
            MapObject(objectMap5, SprocObjectMapList);
            MapObject(objectMap6, SprocObjectMapList);

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
            return result;
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3, T4, T5, T6>(SqlConnection conn, string procName, Action<T, T1, T2, T3, T4, T5, T6> callBack,
    MapObject<T> objectMap = null, MapObject<T1> objectMap2 = null, MapObject<T2> objectMap3 = null, MapObject<T3> objectMap4 = null, MapObject<T4> objectMap5 = null, MapObject<T5> objectMap6 = null, MapObject<T6> objectMap7 = null,
    int commandTimeout = 600)
        {
            SprocObjectMapList = new List<ISprocObjectMap>();

            MapObject(objectMap, SprocObjectMapList);
            MapObject(objectMap2, SprocObjectMapList);
            MapObject(objectMap3, SprocObjectMapList);
            MapObject(objectMap4, SprocObjectMapList);
            MapObject(objectMap5, SprocObjectMapList);
            MapObject(objectMap6, SprocObjectMapList);
            MapObject(objectMap7, SprocObjectMapList);

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
            return result;
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3, T4, T5, T6, T7>(SqlConnection conn, string procName, Action<T, T1, T2, T3, T4, T5, T6, T7> callBack,
MapObject<T> objectMap = null, MapObject<T1> objectMap2 = null, MapObject<T2> objectMap3 = null, MapObject<T3> objectMap4 = null, MapObject<T4> objectMap5 = null, MapObject<T5> objectMap6 = null, MapObject<T6> objectMap7 = null, MapObject<T7> objectMap8 = null,
int commandTimeout = 600)
        {
            SprocObjectMapList = new List<ISprocObjectMap>();

            MapObject(objectMap, SprocObjectMapList);
            MapObject(objectMap2, SprocObjectMapList);
            MapObject(objectMap3, SprocObjectMapList);
            MapObject(objectMap4, SprocObjectMapList);
            MapObject(objectMap5, SprocObjectMapList);
            MapObject(objectMap6, SprocObjectMapList);
            MapObject(objectMap7, SprocObjectMapList);
            MapObject(objectMap8, SprocObjectMapList);

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
            return result;
        }

        private static void MapObject<T>(MapObject<T> map, List<ISprocObjectMap> mapList)
        {
            if (map == null)
                map = PropertyMapper.MapObject<T>();

            map.AddAllColumns();

            mapList.Add(map.GetMap());
        }
    }
}

