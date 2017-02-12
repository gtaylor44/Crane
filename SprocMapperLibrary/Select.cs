using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace SprocMapperLibrary
{
    public class Select : AbstractSelect
    {
        public Select(List<ISprocObjectMap> sprocObjectMapList) : base(sprocObjectMapList)
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

        public IEnumerable<T> ExecuteReader<T1, T2, T>(SqlConnection conn, string procName, Func<T1, T2, T> callBack,
    MapObject<T1> objectMap = null, MapObject<T2> objectMap2 = null,
    int commandTimeout = 600)
        {
            SprocObjectMapList = new List<ISprocObjectMap>();
            //List<ISprocObjectMap> objectMapList = new List<ISprocObjectMap>();

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
                    return default(List<T>);

                while (reader.Read())
                {
                    T1 obj1 = SprocMapperHelper.GetObject<T1>(SprocObjectMapList[0], reader);
                    T2 obj2 = SprocMapperHelper.GetObject<T2>(SprocObjectMapList[1], reader);

                    T obj = callBack.Invoke(obj1, obj2);

                    result.Add(obj);
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

                while (reader.Read())
                {
                    T obj = SprocMapperHelper.GetObject<T>(SprocObjectMapList[0], reader);

                    result.Add(obj);
                }

            }
            return result;

        }

        public IEnumerable<T> ExecuteReader<T1, T2>(SqlConnection conn, string procName, Func<T1, T2, T> callBack,
            MapObject<T1> objectMap = null, MapObject<T2> objectMap2 = null,
            int commandTimeout = 600)
        {
            SprocObjectMapList = new List<ISprocObjectMap>();
            //List<ISprocObjectMap> objectMapList = new List<ISprocObjectMap>();

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
                    return default(List<T>);

                while (reader.Read())
                {
                    T1 obj1 = SprocMapperHelper.GetObject<T1>(SprocObjectMapList[0], reader);
                    T2 obj2 = SprocMapperHelper.GetObject<T2>(SprocObjectMapList[1], reader);

                    T obj = callBack.Invoke(obj1, obj2);

                    result.Add(obj);
                }

            }
            return result;
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

                while (reader.Read())
                {
                    T1 obj1 = SprocMapperHelper.GetObject<T1>(SprocObjectMapList[0], reader);
                    T2 obj2 = SprocMapperHelper.GetObject<T2>(SprocObjectMapList[1], reader);
                    T3 obj3 = SprocMapperHelper.GetObject<T3>(SprocObjectMapList[2], reader);

                    T obj = callBack.Invoke(obj1, obj2, obj3);

                    result.Add(obj);
                }

            }
            return result;
        }

        public IEnumerable<T> ExecuteReader<T1, T2, T3, T4>(SqlConnection conn, string procName, Func<T1, T2, T3, T4, T> callBack,
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

                    T obj = callBack.Invoke(obj1, obj2, obj3, obj4);

                    result.Add(obj);
                }

            }
            return result;
        }

        public IEnumerable<T> ExecuteReader<T1, T2, T3, T4, T5>(SqlConnection conn, string procName, Func<T1, T2, T3, T4, T5, T> callBack,
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

                    T obj = callBack.Invoke(obj1, obj2, obj3, obj4, obj5);

                    result.Add(obj);
                }

            }
            return result;
        }

        public IEnumerable<T> ExecuteReader<T1, T2, T3, T4, T5, T6>(SqlConnection conn, string procName, Func<T1, T2, T3, T4, T5, T6, T> callBack,
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

                    T obj = callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6);

                    result.Add(obj);
                }

            }
            return result;
        }

        public IEnumerable<T> ExecuteReader<T1, T2, T3, T4, T5, T6, T7>(SqlConnection conn, string procName, Func<T1, T2, T3, T4, T5, T6, T7, T> callBack,
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

                    T obj = callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7);

                    result.Add(obj);
                }

            }
            return result;
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

        public IEnumerable<T> ExecuteReader<T1, T2, T3, T4, T5, T6, T7, T8, T9>(SqlConnection conn, string procName, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T> callBack,
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
                    T9 obj9 = SprocMapperHelper.GetObject<T9>(SprocObjectMapList[8], reader);

                    T obj = callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8, obj9);

                    result.Add(obj);
                }

            }
            return result;
        }

        public IEnumerable<T> ExecuteReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(SqlConnection conn, string procName, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T> callBack,
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
                    T9 obj9 = SprocMapperHelper.GetObject<T9>(SprocObjectMapList[8], reader);
                    T10 obj10 = SprocMapperHelper.GetObject<T10>(SprocObjectMapList[9], reader);

                    T obj = callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8, obj9, obj10);

                    result.Add(obj);
                }

            }
            return result;
        }

        public IEnumerable<T> ExecuteReader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(SqlConnection conn, string procName, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T> callBack,
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
                    T9 obj9 = SprocMapperHelper.GetObject<T9>(SprocObjectMapList[8], reader);
                    T10 obj10 = SprocMapperHelper.GetObject<T10>(SprocObjectMapList[9], reader);
                    T11 obj11 = SprocMapperHelper.GetObject<T11>(SprocObjectMapList[10], reader);

                    T obj = callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8, obj9, obj10, obj11);

                    result.Add(obj);
                }

            }
            return result;
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
                    T9 obj9 = SprocMapperHelper.GetObject<T9>(SprocObjectMapList[8], reader);
                    T10 obj10 = SprocMapperHelper.GetObject<T10>(SprocObjectMapList[9], reader);
                    T11 obj11 = SprocMapperHelper.GetObject<T11>(SprocObjectMapList[10], reader);
                    T12 obj12 = SprocMapperHelper.GetObject<T12>(SprocObjectMapList[11], reader);

                    T obj = callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8, obj9, obj10, obj11, obj12);

                    result.Add(obj);
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

