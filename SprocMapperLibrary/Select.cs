using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FastMember;
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

        public Select AddSqlParameter(string parameterName, SqlDbType dbType, object value)
        {
            if (parameterName == null)
                throw new NullReferenceException(nameof(parameterName));

            ParamList.Add(new SqlParameter(parameterName, dbType) { Value = value });
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

                    SetOrdinal(schema, _sprocObjectMapList);
                    ValidateDuplicateSelectAliases(schema, pedanticValidation);
                    ValidateSchema(schema);
                                        
                    if (!reader.HasRows)
                        return (IEnumerable<T>)Activator.CreateInstance(typeof(IEnumerable<T>));

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

                    SetOrdinal(schema, _sprocObjectMapList);

                    ValidateDuplicateSelectAliases(schema, pedanticValidation);
                    ValidateSchema(schema);

                    if (!reader.HasRows)
                        return (IEnumerable<T>)Activator.CreateInstance(typeof(IEnumerable<T>));

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
            MapObject<T, NoMap, NoMap, NoMap, NoMap, NoMap, NoMap, NoMap>();

            return ExecuteReaderImpl<T>((reader, res) =>
            {
                T obj1 = GetObject<T>(_sprocObjectMapList[0], reader);
                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public IEnumerable<T> ExecuteReader<T, T1>(SqlConnection conn, string storedProcedure, Action<T, T1> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {
            MapObject<T, T1, NoMap, NoMap, NoMap, NoMap, NoMap, NoMap>();

            return ExecuteReaderImpl<T>((reader, res) =>
            {
                T obj1 = GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = GetObject<T1>(_sprocObjectMapList[1], reader);

                callBack.Invoke(obj1, obj2);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2>(SqlConnection conn, string storedProcedure, Action<T, T1, T2> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {
            MapObject<T, T1, T2, NoMap, NoMap, NoMap, NoMap, NoMap>();

            return ExecuteReaderImpl<T>((reader, res) =>
            {
                T obj1 = GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = GetObject<T2>(_sprocObjectMapList[2], reader);

                callBack.Invoke(obj1, obj2, obj3);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {
            MapObject<T, T1, T2, T3, NoMap, NoMap, NoMap, NoMap>();

            return ExecuteReaderImpl<T>((reader, res) =>
            {
                T obj1 = GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = GetObject<T3>(_sprocObjectMapList[3], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3, T4>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3, T4> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {
            MapObject<T, T1, T2, T3, T4, NoMap, NoMap, NoMap>();

            return ExecuteReaderImpl<T>((reader, res) =>
            {
                T obj1 = GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = GetObject<T3>(_sprocObjectMapList[3], reader);
                T4 obj5 = GetObject<T4>(_sprocObjectMapList[4], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3, T4, T5>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3, T4, T5> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {
            MapObject<T, T1, T2, T3, T4, T5, NoMap, NoMap>();

            return ExecuteReaderImpl<T>((reader, res) =>
            {
                T obj1 = GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = GetObject<T3>(_sprocObjectMapList[3], reader);
                T4 obj5 = GetObject<T4>(_sprocObjectMapList[4], reader);
                T5 obj6 = GetObject<T5>(_sprocObjectMapList[5], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3, T4, T5, T6>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3, T4, T5, T6> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {
            MapObject<T, T1, T2, T3, T4, T5, T6, NoMap>();

            return ExecuteReaderImpl<T>((reader, result) =>
            {
                T obj1 = GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = GetObject<T3>(_sprocObjectMapList[3], reader);
                T4 obj5 = GetObject<T4>(_sprocObjectMapList[4], reader);
                T5 obj6 = GetObject<T5>(_sprocObjectMapList[5], reader);
                T6 obj7 = GetObject<T6>(_sprocObjectMapList[6], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7);

                result.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3, T4, T5, T6, T7>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3, T4, T5, T6, T7> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {
            MapObject<T, T1, T2, T3, T4, T5, T6, T7>();

            return ExecuteReaderImpl<T>((reader, result) =>
            {
                T obj1 = GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = GetObject<T3>(_sprocObjectMapList[3], reader);
                T4 obj5 = GetObject<T4>(_sprocObjectMapList[4], reader);
                T5 obj6 = GetObject<T5>(_sprocObjectMapList[5], reader);
                T6 obj7 = GetObject<T6>(_sprocObjectMapList[6], reader);
                T7 obj8 = GetObject<T7>(_sprocObjectMapList[7], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8);

                result.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T>(SqlConnection conn, string storedProcedure, int commandTimeout = 600, bool pedanticValidation = false)
        {
            MapObject<T, NoMap, NoMap, NoMap, NoMap, NoMap, NoMap, NoMap>();

            return await ExecuteReaderAsyncImpl<T>((reader, res) =>
            {
                T obj1 = GetObject<T>(_sprocObjectMapList[0], reader);
                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1>(SqlConnection conn, string storedProcedure, Action<T, T1> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {

            MapObject<T, T1, NoMap, NoMap, NoMap, NoMap, NoMap, NoMap>();

            return await ExecuteReaderAsyncImpl<T>((reader, res) =>
            {
                T obj1 = GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = GetObject<T1>(_sprocObjectMapList[1], reader);

                callBack.Invoke(obj1, obj2);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1, T2>(SqlConnection conn, string storedProcedure, Action<T, T1, T2> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {
            MapObject<T, T1, T2, NoMap, NoMap, NoMap, NoMap, NoMap>();

            return await ExecuteReaderAsyncImpl<T>((reader, res) =>
            {
                T obj1 = GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = GetObject<T2>(_sprocObjectMapList[2], reader);

                callBack.Invoke(obj1, obj2, obj3);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1, T2, T3>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {
            MapObject<T, T1, T2, T3, NoMap, NoMap, NoMap, NoMap>();

            return await ExecuteReaderAsyncImpl<T>((reader, res) =>
            {
                T obj1 = GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = GetObject<T3>(_sprocObjectMapList[3], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1, T2, T3, T4>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3, T4> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {
            MapObject<T, T1, T2, T3, T4, NoMap, NoMap, NoMap>();

            return await ExecuteReaderAsyncImpl<T>((reader, res) =>
            {
                T obj1 = GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = GetObject<T3>(_sprocObjectMapList[3], reader);
                T4 obj5 = GetObject<T4>(_sprocObjectMapList[4], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1, T2, T3, T4, T5>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3, T4, T5> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {
            MapObject<T, T1, T2, T3, T4, T5, NoMap, NoMap>();

            return await ExecuteReaderAsyncImpl<T>((reader, res) =>
            {
                T obj1 = GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = GetObject<T3>(_sprocObjectMapList[3], reader);
                T4 obj5 = GetObject<T4>(_sprocObjectMapList[4], reader);
                T5 obj6 = GetObject<T5>(_sprocObjectMapList[5], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1, T2, T3, T4, T5, T6>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3, T4, T5, T6> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {
            MapObject<T, T1, T2, T3, T4, T5, T6, NoMap>();

            return await ExecuteReaderAsyncImpl<T>((reader, res) =>
            {
                T obj1 = GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = GetObject<T3>(_sprocObjectMapList[3], reader);
                T4 obj5 = GetObject<T4>(_sprocObjectMapList[4], reader);
                T5 obj6 = GetObject<T5>(_sprocObjectMapList[5], reader);
                T6 obj7 = GetObject<T6>(_sprocObjectMapList[6], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7);

                res.Add(obj1);

            }, conn, storedProcedure, commandTimeout, pedanticValidation);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1, T2, T3, T4, T5, T6, T7>(SqlConnection conn, string storedProcedure, Action<T, T1, T2, T3, T4, T5, T6, T7> callBack,
            int commandTimeout = 600, bool pedanticValidation = false)
        {

            MapObject<T, T1, T2, T3, T4, T5, T6, T7>();

            return await ExecuteReaderAsyncImpl<T>((reader, res) =>
            {
                T obj1 = GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = GetObject<T3>(_sprocObjectMapList[3], reader);
                T4 obj5 = GetObject<T4>(_sprocObjectMapList[4], reader);
                T5 obj6 = GetObject<T5>(_sprocObjectMapList[5], reader);
                T6 obj7 = GetObject<T6>(_sprocObjectMapList[6], reader);
                T7 obj8 = GetObject<T7>(_sprocObjectMapList[7], reader);

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

        internal void ValidateDuplicateSelectAliases(DataTable schema, bool pedanticValidation)
        {
            HashSet<string> columns = new HashSet<string>(StringComparer.Ordinal);

            var rowList = schema?.Rows.Cast<DataRow>();

            if (rowList != null)
            {
                foreach (var occurence in rowList)
                {
                    string schemaColumn = (string)occurence["ColumnName"];

                    if (columns.Contains(schemaColumn))
                    {
                        throw new SprocMapperException(
                            $"Duplicate column in select not allowed. Ensure that all columns in stored procedure are unique. " +
                            "Try setting an alias for your column in your stored procedure " +
                            $"and set up a custom column mapping. The offending column is '{schemaColumn}'");
                    }

                    columns.Add(schemaColumn);
                }
            }

            if (pedanticValidation)
            {
                ClearUnusedMaps(columns);
            }
        }

        private void ClearUnusedMaps(HashSet<string> columns)
        {
            HashSet<string> allColumns = new HashSet<string>();
            foreach (var map in _sprocObjectMapList)
            {
                map.Columns.ToList().ForEach(x =>
                {
                    if (map.CustomColumnMappings.ContainsKey(x))
                    {
                        if (!columns.Contains(map.CustomColumnMappings[x]))
                        {
                            map.CustomColumnMappings.Remove(map.CustomColumnMappings[x]);
                        }
                        else
                        {
                            allColumns.Add(map.CustomColumnMappings[x]);
                        }
                    }
                    else if (!columns.Contains(x))
                    {

                        map.Columns.Remove(x);
                    }
                    else
                    {
                        allColumns.Add(x);
                    }
                });
            }

            ValidateSelectParams(columns, allColumns);
        }

        private void ValidateSelectParams(HashSet<string> schemaColumnSet, HashSet<string> allColumns)
        {
            HashSet<string> unmatchedParams = new HashSet<string>(StringComparer.Ordinal);
            foreach (var selectParam in schemaColumnSet)
            {
                if (!allColumns.Contains(selectParam))
                    unmatchedParams.Add(selectParam);
            }

            if (unmatchedParams.Count > 0)
            {
                string message = string.Join(", ", unmatchedParams.ToList());
                throw new SprocMapperException($"The following select params are not mapped: {message}");
            }
        }

        internal static void SetOrdinal(DataTable schema, List<ISprocObjectMap> sprocObjectMapList)
        {
            var rowDic = schema?.Rows.Cast<DataRow>().ToDictionary(x => x["ColumnName"].ToString().ToLower());

            if (rowDic == null)
                return;


            foreach (var map in sprocObjectMapList)
            {
                foreach (var column in map.Columns)
                {
                    string actualColumn = column;

                    if (map.CustomColumnMappings.ContainsKey(actualColumn))
                        actualColumn = map.CustomColumnMappings[actualColumn];

                    DataRow dataRow;

                    if (rowDic.TryGetValue(actualColumn.ToLower(), out dataRow))
                    {
                        int ordinalAsInt = int.Parse(dataRow["ColumnOrdinal"].ToString());

                        if (ValidateOrdinal(sprocObjectMapList, actualColumn))
                        {
                            map.ColumnOrdinalDic.Add(actualColumn, ordinalAsInt);
                        }

                    }
                }

            }
        }

        internal static bool ValidateOrdinal(List<ISprocObjectMap> sprocObjectMapList, string key)
        {
            foreach (var map in sprocObjectMapList)
            {
                if (map.ColumnOrdinalDic.ContainsKey(key))
                {                    
                    throw new SprocMapperException($"\n\nThe column '{key}' can't be mapped twice. " +
                                                   $"Ignore this column from the model(s) that is not using this column. The model that currently has this property mapped is " +
                                                   $"'{map.Type.Name}' You can ignore columns using the AddMapping method. Example: AddMapping({typeof(PropertyMapper).Name}.MapObject<{map.Type.Name}>().IgnoreColumn(x => x.{key}))\n\n");
                }
            }

            return true;
        }

        // Validate that no custom column mapping is mapped to 
        public bool ValidateCustomColumnMappings(List<ISprocObjectMap> sprocObjectMapList)
        {
            foreach (var map in sprocObjectMapList)
            {
                foreach (var column in map.CustomColumnMappings)
                {
                    foreach (var mapping in sprocObjectMapList)
                    {
                        if (mapping.Columns.Contains(column.Value)
                            || mapping.CustomColumnMappings.ContainsValue(column.Value))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }


        private void ValidateSchema(DataTable schema)
        {
            var dataRowLIst = schema?.Rows.Cast<DataRow>();

            if (dataRowLIst != null)
            {
                foreach (var occurence in dataRowLIst)
                {
                    string schemaColumn = (string)occurence["ColumnName"];
                    foreach (var map in _sprocObjectMapList)
                    {
                        map.Columns.ToList().ForEach(x =>
                        {
                            if (map.CustomColumnMappings.ContainsKey(x))
                            {
                                if (map.CustomColumnMappings[x].Equals(schemaColumn,
                                    StringComparison.Ordinal))
                                {
                                    ValidateColumn(map, x, occurence);
                                }
                            }
                            else if (x.Equals(schemaColumn,
                                StringComparison.Ordinal))
                            {
                                ValidateColumn(map, x, occurence);

                            }
                        });
                    }
                }
            }
        }

        private void ValidateColumn(ISprocObjectMap map, string schemaColumn, DataRow occurence)
        {
            Member member;

            if (!map.MemberInfoCache.TryGetValue(schemaColumn, out member))
            {
                throw new KeyNotFoundException($"Could not get schema property {schemaColumn}");
            }

            var schemaProperty = (Type)occurence["DataType"];

            Type nullableType;
            if ((nullableType = Nullable.GetUnderlyingType(member.Type)) != null && schemaProperty != nullableType)
            {
                throw new SprocMapperException($"Type mismatch for column '{member.Name}'. Expected type of '{schemaProperty}' but instead saw type '{member.Type}'");
            }

            if (schemaProperty != member.Type && nullableType == null)
            {
                throw new SprocMapperException($"Type mismatch for column '{member.Name}'. Expected type of '{schemaProperty}' but instead saw type '{member.Type}'");
            }
        }

        private void MapObject<T, T1, T2, T3, T4, T5, T6, T7>()
        {
            if (typeof(T) != typeof(NoMap))
                MapObject<T>();

            if (typeof(T1) != typeof(NoMap))
                MapObject<T1>();

            if (typeof(T2) != typeof(NoMap))
                MapObject<T2>();

            if (typeof(T3) != typeof(NoMap))
                MapObject<T3>();

            if (typeof(T4) != typeof(NoMap))
                MapObject<T4>();

            if (typeof(T5) != typeof(NoMap))
                MapObject<T5>();

            if (typeof(T6) != typeof(NoMap))
                MapObject<T6>();

            if (typeof(T7) != typeof(NoMap))
                MapObject<T7>();
        }

        private void MapObject<T>()
        {
            ISprocObjectMap objMap;
            if (!_sprocObjectMapDic.TryGetValue(typeof(T), out objMap))
            {
                var map = PropertyMapper.MapObject<T>();
                map.AddAllColumns();

                objMap = map.GetMap();
            }

            _sprocObjectMapList.Add(objMap);
        }

        public static T GetObject<T>(ISprocObjectMap sprocObjectMap, IDataReader reader)
        {
            T targetObject = NewInstance<T>.Instance();

            foreach (var column in sprocObjectMap.Columns)
            {
                var actualColumn = sprocObjectMap.CustomColumnMappings.ContainsKey(column)
                    ? sprocObjectMap.CustomColumnMappings[column] : column;

                int ordinal;
                if (!sprocObjectMap.ColumnOrdinalDic.TryGetValue(actualColumn, out ordinal))
                    continue;

                Member member;
                if (!sprocObjectMap.MemberInfoCache.TryGetValue(column, out member))
                {
                    throw new KeyNotFoundException($"Could not get property for column {column}");
                }


                object readerObj = reader[ordinal];

                if (readerObj == DBNull.Value)
                {
                    sprocObjectMap.TypeAccessor[targetObject, member.Name] = GetDefaultValue(member);
                }

                else
                {
                    sprocObjectMap.TypeAccessor[targetObject, member.Name] = readerObj;
                }

            }

            return targetObject;
        }

        static object GetDefaultValue(Member member)
        {
            if (member.Type.IsValueType)
                return Activator.CreateInstance(member.Type);
            return null;
        }


        internal static class NewInstance<T>
        {
            public static readonly Func<T> Instance =
                Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile();
        }
    }
}