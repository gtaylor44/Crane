using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using SprocMapperLibrary.CustomException;

namespace SprocMapperLibrary
{
    public class Select : AbstractQuery
    {
        private readonly Dictionary<Type, ISprocObjectMap> _sprocObjectMapDic;
        private readonly List<ISprocObjectMap> _sprocObjectMapList;
        public Select() : base()
        {
            _sprocObjectMapDic = new Dictionary<Type, ISprocObjectMap>();
            _sprocObjectMapList = new List<ISprocObjectMap>();
        }

        public Select AddSqlParameter(SqlParameter item)
        {
            ParamList.Add(item);
            return this;
        }

        public Select AddSqlParameter(string parameterName, object value)
        {
            if (parameterName == null)
                throw new NullReferenceException(nameof(parameterName));

            ParamList.Add(new SqlParameter() { ParameterName = parameterName, Value = value });
            return this;
        }

        public Select AddSqlParameter(string parameterName, object value, SqlDbType dbType)
        {
            if (parameterName == null)
                throw new NullReferenceException(nameof(parameterName));

            ParamList.Add(new SqlParameter() { Value = value, ParameterName = parameterName, SqlDbType = dbType });
            return this;
        }

        public Select AddSqlParameterCollection(IEnumerable<SqlParameter> sqlParameterCollection)
        {
            if (sqlParameterCollection == null)
                throw new NullReferenceException(nameof(sqlParameterCollection));

            ParamList.AddRange(sqlParameterCollection);
            return this;
        }

        internal IEnumerable<T> ExecuteReaderImpl<T>(Action<SqlDataReader, List<T>> getObjectDel, SqlConnection conn, string storedProcedure,
            int commandTimeout, bool pedanticValidation)
        {
            // Try open connection if not already open.
            OpenConn(conn);

            List<T> result = new List<T>();
            using (SqlCommand command = new SqlCommand(storedProcedure, conn))
            {
                // Set common SqlCommand properties
                SetCommandProps(command, commandTimeout);

                using (var reader = command.ExecuteReader())
                {
                    DataTable schema = reader.GetSchemaTable();

                    SprocMapper.ValidateDuplicateSelectAliases(schema, pedanticValidation, _sprocObjectMapList);
                    SprocMapper.SetOrdinal(schema, _sprocObjectMapList);

                    SprocMapper.ValidateSchema(schema, _sprocObjectMapList);

                    if (!reader.HasRows)
                        return (List<T>)Activator.CreateInstance(typeof(List<T>));

                    while (reader.Read())
                    {
                        getObjectDel(reader, result);
                    }
                }

            }
            return result;
        }

        internal async Task<IEnumerable<T>> ExecuteReaderAsyncImpl<T>(Action<SqlDataReader, List<T>> getObjectDel, SqlConnection conn, string storedProcedure,
            int commandTimeout, bool pedanticValidation)
        {
            // Try open connection if not already open.
            await OpenConnAsync(conn);

            List<T> result = new List<T>();

            using (SqlCommand command = new SqlCommand(storedProcedure, conn))
            {
                // Set common SqlCommand properties
                SetCommandProps(command, commandTimeout);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    DataTable schema = reader.GetSchemaTable();

                    SprocMapper.ValidateDuplicateSelectAliases(schema, pedanticValidation, _sprocObjectMapList);
                    SprocMapper.SetOrdinal(schema, _sprocObjectMapList);
                    SprocMapper.ValidateSchema(schema, _sprocObjectMapList);

                    if (!reader.HasRows)
                        return (List<T>)Activator.CreateInstance(typeof(List<T>));

                    while (reader.Read())
                    {
                        getObjectDel(reader, result);
                    }
                }

            }
            return result;
        }

        public IEnumerable<T> ExecuteReader<T>(SqlConnection conn, string storedProcedure, int commandTimeout = 600, bool pedanticValidation = false)
        {
            SprocMapper.MapObject<T, NoMap, NoMap, NoMap, NoMap, NoMap, NoMap, NoMap>(_sprocObjectMapDic, _sprocObjectMapList);

            return ExecuteReaderImpl<T>((reader, res) =>
            {
                T obj1 = SprocMapper.GetObject<T>(_sprocObjectMapList[0], reader);
                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public IEnumerable<T> ExecuteReader<T, T1>(SqlConnection conn, string storedProcedure, Action<T, T1> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {
            SprocMapper.MapObject<T, T1, NoMap, NoMap, NoMap, NoMap, NoMap, NoMap>(_sprocObjectMapDic, _sprocObjectMapList);

            return ExecuteReaderImpl<T>((reader, res) =>
            {
                T obj1 = SprocMapper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapper.GetObject<T1>(_sprocObjectMapList[1], reader);

                callBack.Invoke(obj1, obj2);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2>(SqlConnection conn, string storedProcedure, Action<T, T1, T2> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {
            SprocMapper.MapObject<T, T1, T2, NoMap, NoMap, NoMap, NoMap, NoMap>(_sprocObjectMapDic, _sprocObjectMapList);

            return ExecuteReaderImpl<T>((reader, res) =>
            {
                T obj1 = SprocMapper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapper.GetObject<T2>(_sprocObjectMapList[2], reader);

                callBack.Invoke(obj1, obj2, obj3);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {
            SprocMapper.MapObject<T, T1, T2, T3, NoMap, NoMap, NoMap, NoMap>(_sprocObjectMapDic, _sprocObjectMapList);

            return ExecuteReaderImpl<T>((reader, res) =>
            {
                T obj1 = SprocMapper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapper.GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = SprocMapper.GetObject<T3>(_sprocObjectMapList[3], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3, T4>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3, T4> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {
            SprocMapper.MapObject<T, T1, T2, T3, T4, NoMap, NoMap, NoMap>(_sprocObjectMapDic, _sprocObjectMapList);

            return ExecuteReaderImpl<T>((reader, res) =>
            {
                T obj1 = SprocMapper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapper.GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = SprocMapper.GetObject<T3>(_sprocObjectMapList[3], reader);
                T4 obj5 = SprocMapper.GetObject<T4>(_sprocObjectMapList[4], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3, T4, T5>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3, T4, T5> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {
            SprocMapper.MapObject<T, T1, T2, T3, T4, T5, NoMap, NoMap>(_sprocObjectMapDic, _sprocObjectMapList);

            return ExecuteReaderImpl<T>((reader, res) =>
            {
                T obj1 = SprocMapper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapper.GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = SprocMapper.GetObject<T3>(_sprocObjectMapList[3], reader);
                T4 obj5 = SprocMapper.GetObject<T4>(_sprocObjectMapList[4], reader);
                T5 obj6 = SprocMapper.GetObject<T5>(_sprocObjectMapList[5], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3, T4, T5, T6>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3, T4, T5, T6> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {
            SprocMapper.MapObject<T, T1, T2, T3, T4, T5, T6, NoMap>(_sprocObjectMapDic, _sprocObjectMapList);

            return ExecuteReaderImpl<T>((reader, result) =>
            {
                T obj1 = SprocMapper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapper.GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = SprocMapper.GetObject<T3>(_sprocObjectMapList[3], reader);
                T4 obj5 = SprocMapper.GetObject<T4>(_sprocObjectMapList[4], reader);
                T5 obj6 = SprocMapper.GetObject<T5>(_sprocObjectMapList[5], reader);
                T6 obj7 = SprocMapper.GetObject<T6>(_sprocObjectMapList[6], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7);

                result.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3, T4, T5, T6, T7>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3, T4, T5, T6, T7> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {
            SprocMapper.MapObject<T, T1, T2, T3, T4, T5, T6, T7>(_sprocObjectMapDic, _sprocObjectMapList);

            return ExecuteReaderImpl<T>((reader, result) =>
            {
                T obj1 = SprocMapper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapper.GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = SprocMapper.GetObject<T3>(_sprocObjectMapList[3], reader);
                T4 obj5 = SprocMapper.GetObject<T4>(_sprocObjectMapList[4], reader);
                T5 obj6 = SprocMapper.GetObject<T5>(_sprocObjectMapList[5], reader);
                T6 obj7 = SprocMapper.GetObject<T6>(_sprocObjectMapList[6], reader);
                T7 obj8 = SprocMapper.GetObject<T7>(_sprocObjectMapList[7], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8);

                result.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T>(SqlConnection conn, string storedProcedure, int commandTimeout = 600, bool pedanticValidation = false)
        {
            SprocMapper.MapObject<T, NoMap, NoMap, NoMap, NoMap, NoMap, NoMap, NoMap>(_sprocObjectMapDic, _sprocObjectMapList);

            return await ExecuteReaderAsyncImpl<T>((reader, res) =>
            {
                T obj1 = SprocMapper.GetObject<T>(_sprocObjectMapList[0], reader);
                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1>(SqlConnection conn, string storedProcedure, Action<T, T1> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {

            SprocMapper.MapObject<T, T1, NoMap, NoMap, NoMap, NoMap, NoMap, NoMap>(_sprocObjectMapDic, _sprocObjectMapList);

            return await ExecuteReaderAsyncImpl<T>((reader, res) =>
            {
                T obj1 = SprocMapper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapper.GetObject<T1>(_sprocObjectMapList[1], reader);

                callBack.Invoke(obj1, obj2);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1, T2>(SqlConnection conn, string storedProcedure, Action<T, T1, T2> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {
            SprocMapper.MapObject<T, T1, T2, NoMap, NoMap, NoMap, NoMap, NoMap>(_sprocObjectMapDic, _sprocObjectMapList);

            return await ExecuteReaderAsyncImpl<T>((reader, res) =>
            {
                T obj1 = SprocMapper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapper.GetObject<T2>(_sprocObjectMapList[2], reader);

                callBack.Invoke(obj1, obj2, obj3);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1, T2, T3>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {
            SprocMapper.MapObject<T, T1, T2, T3, NoMap, NoMap, NoMap, NoMap>(_sprocObjectMapDic, _sprocObjectMapList);

            return await ExecuteReaderAsyncImpl<T>((reader, res) =>
            {
                T obj1 = SprocMapper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapper.GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = SprocMapper.GetObject<T3>(_sprocObjectMapList[3], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1, T2, T3, T4>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3, T4> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {
            SprocMapper.MapObject<T, T1, T2, T3, T4, NoMap, NoMap, NoMap>(_sprocObjectMapDic, _sprocObjectMapList);

            return await ExecuteReaderAsyncImpl<T>((reader, res) =>
            {
                T obj1 = SprocMapper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapper.GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = SprocMapper.GetObject<T3>(_sprocObjectMapList[3], reader);
                T4 obj5 = SprocMapper.GetObject<T4>(_sprocObjectMapList[4], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1, T2, T3, T4, T5>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3, T4, T5> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {
            SprocMapper.MapObject<T, T1, T2, T3, T4, T5, NoMap, NoMap>(_sprocObjectMapDic, _sprocObjectMapList);

            return await ExecuteReaderAsyncImpl<T>((reader, res) =>
            {
                T obj1 = SprocMapper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapper.GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = SprocMapper.GetObject<T3>(_sprocObjectMapList[3], reader);
                T4 obj5 = SprocMapper.GetObject<T4>(_sprocObjectMapList[4], reader);
                T5 obj6 = SprocMapper.GetObject<T5>(_sprocObjectMapList[5], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1, T2, T3, T4, T5, T6>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3, T4, T5, T6> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {
            SprocMapper.MapObject<T, T1, T2, T3, T4, T5, T6, NoMap>(_sprocObjectMapDic, _sprocObjectMapList);

            return await ExecuteReaderAsyncImpl<T>((reader, res) =>
            {
                T obj1 = SprocMapper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapper.GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = SprocMapper.GetObject<T3>(_sprocObjectMapList[3], reader);
                T4 obj5 = SprocMapper.GetObject<T4>(_sprocObjectMapList[4], reader);
                T5 obj6 = SprocMapper.GetObject<T5>(_sprocObjectMapList[5], reader);
                T6 obj7 = SprocMapper.GetObject<T6>(_sprocObjectMapList[6], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1, T2, T3, T4, T5, T6, T7>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3, T4, T5, T6, T7> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {

            SprocMapper.MapObject<T, T1, T2, T3, T4, T5, T6, T7>(_sprocObjectMapDic, _sprocObjectMapList);

            return await ExecuteReaderAsyncImpl<T>((reader, res) =>
            {
                T obj1 = SprocMapper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapper.GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = SprocMapper.GetObject<T3>(_sprocObjectMapList[3], reader);
                T4 obj5 = SprocMapper.GetObject<T4>(_sprocObjectMapList[4], reader);
                T5 obj6 = SprocMapper.GetObject<T5>(_sprocObjectMapList[5], reader);
                T6 obj7 = SprocMapper.GetObject<T6>(_sprocObjectMapList[6], reader);
                T7 obj8 = SprocMapper.GetObject<T7>(_sprocObjectMapList[7], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public Select AddMapping<T>(MapObject<T> propertyMap)
        {
            if (propertyMap == null)
                throw new NullReferenceException(nameof(propertyMap));

            if (typeof(T).IsValueType)
                throw new SprocMapperException("An error occurred in AddMapping<T>. Map can't be value type, must be custom.");

            if (!_sprocObjectMapDic.ContainsKey(typeof(T)))
            {
                propertyMap.AddAllColumns();
                _sprocObjectMapDic.Add(typeof(T), propertyMap.GetMap());
            }
            else
            {
                throw new SprocMapperException($"An error occurred in AddMapping<T>. Map already exists for type {typeof(T).Name}");
            }

            return this;
        }

        
    }
}