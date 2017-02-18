using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FastMember;
using SprocMapperLibrary.Core;

namespace SprocMapperLibrary
{
    public class Select : AbstractQuery
    {
        private readonly List<ISprocObjectMap> _sprocObjectMapList;
        private readonly Dictionary<Type, Dictionary<string, string>> _customColumnMappings;
        public Select() : base()
        {
            _sprocObjectMapList = new List<ISprocObjectMap>();
            _customColumnMappings = new Dictionary<Type, Dictionary<string, string>>();
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

        public Select CustomColumnMapping<T>(Expression<Func<T, object>> source, string destination)
        {
            var propertyName = SprocMapper.GetPropertyName(source);

            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            if (typeof(T).IsValueType)
                throw new SprocMapperException("An error occurred in CustomColumnMapping<T>. Can't be value type");

            Dictionary<string, string> customColumnDic;
            if (!_customColumnMappings.TryGetValue(typeof(T), out customColumnDic))
            {
                Dictionary<string, string> newDic = new Dictionary<string, string>();
                _customColumnMappings.Add(typeof(T), newDic);

                customColumnDic = newDic;
            }        

            var typeAccessor = TypeAccessor.Create(typeof(T));

            //Get all properties
            MemberSet members = typeAccessor.GetMembers();

            foreach (var member in members)
            {
                if (member.Name.Equals(destination, StringComparison.OrdinalIgnoreCase))
                    throw new SprocMapperException($"Custom column mapping must map to a unique " +
                                                   $"property. A property with the name '{destination}' already exists.");
            }

            customColumnDic.Add(propertyName, destination);

            return this;
        }

        internal IEnumerable<T> ExecuteReaderImpl<T>(Action<SqlDataReader, List<T>> getObjectDel, SqlConnection conn, string storedProcedure,
            int? commandTimeout, string partitionOn, bool validatePartitionOn)
        {
            if (validatePartitionOn)
                SprocMapper.ValidatePartitionOn(partitionOn);

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
                    SprocMapper.SetOrdinal(schema, _sprocObjectMapList, partitionOn);
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
            int? commandTimeout, string partitionOn, bool validatePartitionOn)
        {
            if (validatePartitionOn)
                SprocMapper.ValidatePartitionOn(partitionOn);

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
                    SprocMapper.SetOrdinal(schema, _sprocObjectMapList, partitionOn);
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

        public IEnumerable<T> ExecuteReader<T>(SqlConnection conn, string storedProcedure, int? commandTimeout = null)
        {
            SprocMapper.MapObject<T, NoMap, NoMap, NoMap, NoMap, NoMap, NoMap, NoMap>(_sprocObjectMapList, _customColumnMappings);

            return ExecuteReaderImpl<T>((reader, res) =>
            {
                T obj1 = SprocMapper.GetObject<T>(_sprocObjectMapList[0], reader);
                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, null, false);
        }

        public IEnumerable<T> ExecuteReader<T, T1>(SqlConnection conn, string storedProcedure, Action<T, T1> callBack,
            string partitionOn, int? commandTimeout = null)
        {
            SprocMapper.MapObject<T, T1, NoMap, NoMap, NoMap, NoMap, NoMap, NoMap>(_sprocObjectMapList, _customColumnMappings);

            return ExecuteReaderImpl<T>((reader, res) =>
            {
                T obj1 = SprocMapper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapper.GetObject<T1>(_sprocObjectMapList[1], reader);

                callBack.Invoke(obj1, obj2);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, partitionOn, true);
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2>(SqlConnection conn, string storedProcedure, Action<T, T1, T2> callBack, string partitionOn,
            int? commandTimeout = null)
        {
            SprocMapper.MapObject<T, T1, T2, NoMap, NoMap, NoMap, NoMap, NoMap>(_sprocObjectMapList, _customColumnMappings);

            return ExecuteReaderImpl<T>((reader, res) =>
            {
                T obj1 = SprocMapper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapper.GetObject<T2>(_sprocObjectMapList[2], reader);

                callBack.Invoke(obj1, obj2, obj3);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, partitionOn, true);
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3> callBack, string partitionOn,
            int? commandTimeout = null)
        {
            SprocMapper.MapObject<T, T1, T2, T3, NoMap, NoMap, NoMap, NoMap>(_sprocObjectMapList, _customColumnMappings);

            return ExecuteReaderImpl<T>((reader, res) =>
            {
                T obj1 = SprocMapper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapper.GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = SprocMapper.GetObject<T3>(_sprocObjectMapList[3], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, partitionOn, true);
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3, T4>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3, T4> callBack, string partitionOn,
            int? commandTimeout = null)
        {
            SprocMapper.MapObject<T, T1, T2, T3, T4, NoMap, NoMap, NoMap>(_sprocObjectMapList, _customColumnMappings);

            return ExecuteReaderImpl<T>((reader, res) =>
            {
                T obj1 = SprocMapper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapper.GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = SprocMapper.GetObject<T3>(_sprocObjectMapList[3], reader);
                T4 obj5 = SprocMapper.GetObject<T4>(_sprocObjectMapList[4], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, partitionOn, true);
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3, T4, T5>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3, T4, T5> callBack, string partitionOn,
            int? commandTimeout = null)
        {
            SprocMapper.MapObject<T, T1, T2, T3, T4, T5, NoMap, NoMap>(_sprocObjectMapList, _customColumnMappings);

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

            }, conn, storedProcedure, commandTimeout, partitionOn, true);
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3, T4, T5, T6>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3, T4, T5, T6> callBack, string partitionOn,
            int? commandTimeout = null)
        {
            SprocMapper.MapObject<T, T1, T2, T3, T4, T5, T6, NoMap>(_sprocObjectMapList, _customColumnMappings);

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

            }, conn, storedProcedure, commandTimeout, partitionOn, true);
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3, T4, T5, T6, T7>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3, T4, T5, T6, T7> callBack, string partitionOn,
            int? commandTimeout)
        {
            SprocMapper.MapObject<T, T1, T2, T3, T4, T5, T6, T7>(_sprocObjectMapList, _customColumnMappings);

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

            }, conn, storedProcedure, commandTimeout, partitionOn, true);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T>(SqlConnection conn, string storedProcedure, int? commandTimeout = null)
        {
            SprocMapper.MapObject<T, NoMap, NoMap, NoMap, NoMap, NoMap, NoMap, NoMap>(_sprocObjectMapList, _customColumnMappings);

            return await ExecuteReaderAsyncImpl<T>((reader, res) =>
            {
                T obj1 = SprocMapper.GetObject<T>(_sprocObjectMapList[0], reader);
                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, null, false);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1>(SqlConnection conn, string storedProcedure, Action<T, T1> callBack, string partitionOn,
            int? commandTimeout = null)
        {

            SprocMapper.MapObject<T, T1, NoMap, NoMap, NoMap, NoMap, NoMap, NoMap>(_sprocObjectMapList, _customColumnMappings);

            return await ExecuteReaderAsyncImpl<T>((reader, res) =>
            {
                T obj1 = SprocMapper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapper.GetObject<T1>(_sprocObjectMapList[1], reader);

                callBack.Invoke(obj1, obj2);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, partitionOn, true);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1, T2>(SqlConnection conn, string storedProcedure, Action<T, T1, T2> callBack, string partitionOn,
            int? commandTimeout = null)
        {
            SprocMapper.MapObject<T, T1, T2, NoMap, NoMap, NoMap, NoMap, NoMap>(_sprocObjectMapList, _customColumnMappings);

            return await ExecuteReaderAsyncImpl<T>((reader, res) =>
            {
                T obj1 = SprocMapper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapper.GetObject<T2>(_sprocObjectMapList[2], reader);

                callBack.Invoke(obj1, obj2, obj3);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, partitionOn, true);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1, T2, T3>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3> callBack, string partitionOn,
            int? commandTimeout = null)
        {
            SprocMapper.MapObject<T, T1, T2, T3, NoMap, NoMap, NoMap, NoMap>(_sprocObjectMapList, _customColumnMappings);

            return await ExecuteReaderAsyncImpl<T>((reader, res) =>
            {
                T obj1 = SprocMapper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapper.GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = SprocMapper.GetObject<T3>(_sprocObjectMapList[3], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, partitionOn, true);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1, T2, T3, T4>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3, T4> callBack, string partitionOn,
            int? commandTimeout = null)
        {
            SprocMapper.MapObject<T, T1, T2, T3, T4, NoMap, NoMap, NoMap>(_sprocObjectMapList, _customColumnMappings);

            return await ExecuteReaderAsyncImpl<T>((reader, res) =>
            {
                T obj1 = SprocMapper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapper.GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = SprocMapper.GetObject<T3>(_sprocObjectMapList[3], reader);
                T4 obj5 = SprocMapper.GetObject<T4>(_sprocObjectMapList[4], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, partitionOn, true);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1, T2, T3, T4, T5>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3, T4, T5> callBack, string partitionOn,
            int? commandTimeout = null)
        {
            SprocMapper.MapObject<T, T1, T2, T3, T4, T5, NoMap, NoMap>(_sprocObjectMapList, _customColumnMappings);

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

            }, conn, storedProcedure, commandTimeout, partitionOn, true);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1, T2, T3, T4, T5, T6>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3, T4, T5, T6> callBack, string partitionOn,
            int? commandTimeout = null)
        {
            SprocMapper.MapObject<T, T1, T2, T3, T4, T5, T6, NoMap>(_sprocObjectMapList, _customColumnMappings);

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

            }, conn, storedProcedure, commandTimeout, partitionOn, true);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1, T2, T3, T4, T5, T6, T7>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3, T4, T5, T6, T7> callBack, string partitionOn,
            int? commandTimeout = null)
        {

            SprocMapper.MapObject<T, T1, T2, T3, T4, T5, T6, T7>(_sprocObjectMapList, _customColumnMappings);

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

            }, conn, storedProcedure, commandTimeout, partitionOn, true);
        }    
    }
}