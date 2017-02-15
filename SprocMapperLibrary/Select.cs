using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
            ParamList.Add(new SqlParameter(parameterName, dbType) { Value = value });
            return this;
        }

        internal IEnumerable<T> ExecuteReaderSynchronously<T>(Action<SqlDataReader, List<T>> getObjectDel, SqlConnection conn, string procName,
            int commandTimeout, bool strictValidation)
        {
            OpenConn(conn);

            List<T> result = new List<T>();
            using (SqlCommand command = new SqlCommand(procName, conn))
            {
                SetCommandProps(command, commandTimeout);

                using (var reader = command.ExecuteReader())
                {
                    DataTable schema = reader.GetSchemaTable();
                  
                    ValidateSchema(schema);
                    ValidateSelectColumns(schema);
                    SetOrdinal(schema, _sprocObjectMapList);

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

        internal async Task<IEnumerable<T>> ExecuteReaderAsynchronously<T>(Action<SqlDataReader, List<T>> getObjectDel, SqlConnection conn, string procName,
            int commandTimeout, bool strictValidation)
        {
            await OpenConnAsync(conn);

            List<T> result = new List<T>();
            using (SqlCommand command = new SqlCommand(procName, conn))
            {
                SetCommandProps(command, commandTimeout);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    DataTable schema = reader.GetSchemaTable();

                    ValidateSchema(schema);
                    ValidateSelectColumns(schema);
                    SetOrdinal(schema, _sprocObjectMapList);

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

        public IEnumerable<T> ExecuteReader<T>(SqlConnection conn, string procName, int commandTimeout = 600, bool strictValidation = false)
        {
            MapObject<T>();

            return ExecuteReaderSynchronously<T>((reader, res) =>
            {
                T obj1 = SprocMapperHelper.GetObject<T>(_sprocObjectMapList[0], reader);
                res.Add(obj1);

            }, conn, procName, commandTimeout, strictValidation);
        }

        public IEnumerable<T> ExecuteReader<T, T1>(SqlConnection conn, string procName, Action<T, T1> callBack,
            int commandTimeout = 600, bool strictValidation = false)
        {

            MapObject<T>();
            MapObject<T1>();

            return ExecuteReaderSynchronously<T>((reader, res) =>
            {
                T obj1 = SprocMapperHelper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapperHelper.GetObject<T1>(_sprocObjectMapList[1], reader);

                callBack.Invoke(obj1, obj2);

                res.Add(obj1);

            }, conn, procName, commandTimeout, strictValidation);
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2>(SqlConnection conn, string procName, Action<T, T1, T2> callBack,
            int commandTimeout = 600, bool strictValidation = false)
        {
            MapObject<T>();
            MapObject<T1>();
            MapObject<T2>();

            return ExecuteReaderSynchronously<T>((reader, res) =>
            {
                T obj1 = SprocMapperHelper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapperHelper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapperHelper.GetObject<T2>(_sprocObjectMapList[2], reader);

                callBack.Invoke(obj1, obj2, obj3);

                res.Add(obj1);

            }, conn, procName, commandTimeout, strictValidation);
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3>(SqlConnection conn, string procName, Action<T, T1, T2, T3> callBack,
            int commandTimeout = 600, bool strictValidation = false)
        {
            MapObject<T>();
            MapObject<T1>();
            MapObject<T2>();
            MapObject<T3>();

            return ExecuteReaderSynchronously<T>((reader, res) =>
            {
                T obj1 = SprocMapperHelper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapperHelper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapperHelper.GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = SprocMapperHelper.GetObject<T3>(_sprocObjectMapList[3], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4);

                res.Add(obj1);

            }, conn, procName, commandTimeout, strictValidation);
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3, T4>(SqlConnection conn, string procName, Action<T, T1, T2, T3, T4> callBack,
            int commandTimeout = 600, bool strictValidation = false)
        {
            MapObject<T>();
            MapObject<T1>();
            MapObject<T2>();
            MapObject<T3>();
            MapObject<T4>();

            return ExecuteReaderSynchronously<T>((reader, res) =>
            {
                T obj1 = SprocMapperHelper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapperHelper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapperHelper.GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = SprocMapperHelper.GetObject<T3>(_sprocObjectMapList[3], reader);
                T4 obj5 = SprocMapperHelper.GetObject<T4>(_sprocObjectMapList[4], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5);

                res.Add(obj1);

            }, conn, procName, commandTimeout, strictValidation);
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3, T4, T5>(SqlConnection conn, string procName, Action<T, T1, T2, T3, T4, T5> callBack,
            int commandTimeout = 600, bool strictValidation = false)
        {
            MapObject<T>();
            MapObject<T1>();
            MapObject<T2>();
            MapObject<T3>();
            MapObject<T4>();
            MapObject<T5>();

            return ExecuteReaderSynchronously<T>((reader, res) =>
            {
                T obj1 = SprocMapperHelper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapperHelper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapperHelper.GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = SprocMapperHelper.GetObject<T3>(_sprocObjectMapList[3], reader);
                T4 obj5 = SprocMapperHelper.GetObject<T4>(_sprocObjectMapList[4], reader);
                T5 obj6 = SprocMapperHelper.GetObject<T5>(_sprocObjectMapList[5], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6);

                res.Add(obj1);

            }, conn, procName, commandTimeout, strictValidation);
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3, T4, T5, T6>(SqlConnection conn, string procName, Action<T, T1, T2, T3, T4, T5, T6> callBack,
            int commandTimeout = 600, bool strictValidation = false)
        {
            MapObject<T>();
            MapObject<T1>();
            MapObject<T2>();
            MapObject<T3>();
            MapObject<T4>();
            MapObject<T5>();
            MapObject<T6>();

            return ExecuteReaderSynchronously<T>((reader, res) =>
            {
                T obj1 = SprocMapperHelper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapperHelper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapperHelper.GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = SprocMapperHelper.GetObject<T3>(_sprocObjectMapList[3], reader);
                T4 obj5 = SprocMapperHelper.GetObject<T4>(_sprocObjectMapList[4], reader);
                T5 obj6 = SprocMapperHelper.GetObject<T5>(_sprocObjectMapList[5], reader);
                T6 obj7 = SprocMapperHelper.GetObject<T6>(_sprocObjectMapList[6], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7);

                res.Add(obj1);

            }, conn, procName, commandTimeout, strictValidation);
        }

        public IEnumerable<T> ExecuteReader<T, T1, T2, T3, T4, T5, T6, T7>(SqlConnection conn, string procName, Action<T, T1, T2, T3, T4, T5, T6, T7> callBack,
            int commandTimeout = 600, bool strictValidation = false)
        {

            MapObject<T>();
            MapObject<T1>();
            MapObject<T2>();
            MapObject<T3>();
            MapObject<T4>();
            MapObject<T5>();
            MapObject<T6>();
            MapObject<T7>();

            return ExecuteReaderSynchronously<T>((reader, res) =>
            {
                T obj1 = SprocMapperHelper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapperHelper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapperHelper.GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = SprocMapperHelper.GetObject<T3>(_sprocObjectMapList[3], reader);
                T4 obj5 = SprocMapperHelper.GetObject<T4>(_sprocObjectMapList[4], reader);
                T5 obj6 = SprocMapperHelper.GetObject<T5>(_sprocObjectMapList[5], reader);
                T6 obj7 = SprocMapperHelper.GetObject<T6>(_sprocObjectMapList[6], reader);
                T7 obj8 = SprocMapperHelper.GetObject<T7>(_sprocObjectMapList[7], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8);

                res.Add(obj1);

            }, conn, procName, commandTimeout, strictValidation);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T>(SqlConnection conn, string procName, int commandTimeout = 600, bool strictValidation = false)
        {
            MapObject<T>();

            return await ExecuteReaderAsynchronously<T>((reader, res) =>
            {
                T obj1 = SprocMapperHelper.GetObject<T>(_sprocObjectMapList[0], reader);
                res.Add(obj1);

            }, conn, procName, commandTimeout, strictValidation);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1>(SqlConnection conn, string procName, Action<T, T1> callBack,
            int commandTimeout = 600, bool strictValidation = false)
        {

            MapObject<T>();
            MapObject<T1>();

            return await ExecuteReaderAsynchronously<T>((reader, res) =>
            {
                T obj1 = SprocMapperHelper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapperHelper.GetObject<T1>(_sprocObjectMapList[1], reader);

                callBack.Invoke(obj1, obj2);

                res.Add(obj1);

            }, conn, procName, commandTimeout, strictValidation);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1, T2>(SqlConnection conn, string procName, Action<T, T1, T2> callBack,
            int commandTimeout = 600, bool strictValidation = false)
        {
            MapObject<T>();
            MapObject<T1>();
            MapObject<T2>();

            return await ExecuteReaderAsynchronously<T>((reader, res) =>
            {
                T obj1 = SprocMapperHelper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapperHelper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapperHelper.GetObject<T2>(_sprocObjectMapList[2], reader);

                callBack.Invoke(obj1, obj2, obj3);

                res.Add(obj1);

            }, conn, procName, commandTimeout, strictValidation);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1, T2, T3>(SqlConnection conn, string procName, Action<T, T1, T2, T3> callBack,
            int commandTimeout = 600, bool strictValidation = false)
        {
            MapObject<T>();
            MapObject<T1>();
            MapObject<T2>();
            MapObject<T3>();

            return await ExecuteReaderAsynchronously<T>((reader, res) =>
            {
                T obj1 = SprocMapperHelper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapperHelper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapperHelper.GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = SprocMapperHelper.GetObject<T3>(_sprocObjectMapList[3], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4);

                res.Add(obj1);

            }, conn, procName, commandTimeout, strictValidation);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1, T2, T3, T4>(SqlConnection conn, string procName, Action<T, T1, T2, T3, T4> callBack,
            int commandTimeout = 600, bool strictValidation = false)
        {
            MapObject<T>();
            MapObject<T1>();
            MapObject<T2>();
            MapObject<T3>();
            MapObject<T4>();

            return await ExecuteReaderAsynchronously<T>((reader, res) =>
            {
                T obj1 = SprocMapperHelper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapperHelper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapperHelper.GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = SprocMapperHelper.GetObject<T3>(_sprocObjectMapList[3], reader);
                T4 obj5 = SprocMapperHelper.GetObject<T4>(_sprocObjectMapList[4], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5);

                res.Add(obj1);

            }, conn, procName, commandTimeout, strictValidation);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1, T2, T3, T4, T5>(SqlConnection conn, string procName, Action<T, T1, T2, T3, T4, T5> callBack,
            int commandTimeout = 600, bool strictValidation = false)
        {
            MapObject<T>();
            MapObject<T1>();
            MapObject<T2>();
            MapObject<T3>();
            MapObject<T4>();
            MapObject<T5>();

            return await ExecuteReaderAsynchronously<T>((reader, res) =>
            {
                T obj1 = SprocMapperHelper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapperHelper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapperHelper.GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = SprocMapperHelper.GetObject<T3>(_sprocObjectMapList[3], reader);
                T4 obj5 = SprocMapperHelper.GetObject<T4>(_sprocObjectMapList[4], reader);
                T5 obj6 = SprocMapperHelper.GetObject<T5>(_sprocObjectMapList[5], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6);

                res.Add(obj1);

            }, conn, procName, commandTimeout, strictValidation);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1, T2, T3, T4, T5, T6>(SqlConnection conn, string procName, Action<T, T1, T2, T3, T4, T5, T6> callBack,
            int commandTimeout = 600, bool strictValidation = false)
        {
            MapObject<T>();
            MapObject<T1>();
            MapObject<T2>();
            MapObject<T3>();
            MapObject<T4>();
            MapObject<T5>();
            MapObject<T6>();

            return await ExecuteReaderAsynchronously<T>((reader, res) =>
            {
                T obj1 = SprocMapperHelper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapperHelper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapperHelper.GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = SprocMapperHelper.GetObject<T3>(_sprocObjectMapList[3], reader);
                T4 obj5 = SprocMapperHelper.GetObject<T4>(_sprocObjectMapList[4], reader);
                T5 obj6 = SprocMapperHelper.GetObject<T5>(_sprocObjectMapList[5], reader);
                T6 obj7 = SprocMapperHelper.GetObject<T6>(_sprocObjectMapList[6], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7);

                res.Add(obj1);

            }, conn, procName, commandTimeout, strictValidation);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T, T1, T2, T3, T4, T5, T6, T7>(SqlConnection conn, string procName, Action<T, T1, T2, T3, T4, T5, T6, T7> callBack,
            int commandTimeout = 600, bool strictValidation = false)
        {

            MapObject<T>();
            MapObject<T1>();
            MapObject<T2>();
            MapObject<T3>();
            MapObject<T4>();
            MapObject<T5>();
            MapObject<T6>();
            MapObject<T7>();

            return await ExecuteReaderAsynchronously<T>((reader, res) =>
            {
                T obj1 = SprocMapperHelper.GetObject<T>(_sprocObjectMapList[0], reader);
                T1 obj2 = SprocMapperHelper.GetObject<T1>(_sprocObjectMapList[1], reader);
                T2 obj3 = SprocMapperHelper.GetObject<T2>(_sprocObjectMapList[2], reader);
                T3 obj4 = SprocMapperHelper.GetObject<T3>(_sprocObjectMapList[3], reader);
                T4 obj5 = SprocMapperHelper.GetObject<T4>(_sprocObjectMapList[4], reader);
                T5 obj6 = SprocMapperHelper.GetObject<T5>(_sprocObjectMapList[5], reader);
                T6 obj7 = SprocMapperHelper.GetObject<T6>(_sprocObjectMapList[6], reader);
                T7 obj8 = SprocMapperHelper.GetObject<T7>(_sprocObjectMapList[7], reader);

                callBack.Invoke(obj1, obj2, obj3, obj4, obj5, obj6, obj7, obj8);

                res.Add(obj1);

            }, conn, procName, commandTimeout, strictValidation);
        }

        public Select AddMapping<T>(MapObject<T> propertyMap)
        {
            if (propertyMap == null)
                throw new NullReferenceException(nameof(propertyMap));

            if (typeof(T).IsValueType)
                throw new SprocMapperException("An error occured in AddMapping<T>. Map can't be value type, must be custom.");

            if (!_sprocObjectMapDic.ContainsKey(typeof(T)))
            {
                propertyMap.AddAllColumns();
                _sprocObjectMapDic.Add(typeof(T), propertyMap.GetMap());
            }
            else
            {
                throw new SprocMapperException($"An error occured in AddMapping<T>. Map already exists for type {typeof(T).Name}");
            }

            return this;
        }

        internal void ValidateSelectColumns(DataTable schema)
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

            //HashSet<string> allColumns = new HashSet<string>();
            //foreach (var map in _sprocObjectMapList)
            //{
            //    map.Columns.ToList().ForEach(x =>
            //    {
            //        if (map.CustomColumnMappings.ContainsKey(x))
            //        {
            //            if (!columns.Contains(map.CustomColumnMappings[x]))
            //            {
            //                map.CustomColumnMappings.Remove(map.CustomColumnMappings[x]);
            //            }
            //            else
            //            {
            //                allColumns.Add(map.CustomColumnMappings[x]);
            //            }
            //        }
            //        else if (!columns.Contains(x))
            //        {

            //            map.Columns.Remove(x);
            //        }
            //        else
            //        {
            //            allColumns.Add(x);
            //        }
            //    });
            //}

            //ValidateSelectParams(columns, allColumns);
        }

        //private void ValidateSelectParams(HashSet<string> schemaColumnSet, HashSet<string> allColumns)
        //{
        //    HashSet<string> unmatchedParams = new HashSet<string>(StringComparer.Ordinal);
        //    foreach (var selectParam in schemaColumnSet)
        //    {
        //        if (!allColumns.Contains(selectParam))
        //            unmatchedParams.Add(selectParam);
        //    }

        //    if (unmatchedParams.Count > 0)
        //    {
        //        string message = string.Join(", ", unmatchedParams.ToList());
        //        throw new SprocMapperException($"The following select params are not mapped: {message}");
        //    }
        //}

        internal void SetOrdinal(DataTable schema, List<ISprocObjectMap> sprocObjectMapList)
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

                        if (!map.ColumnOrdinalDic.ContainsKey(actualColumn))
                        {
                            map.ColumnOrdinalDic.Add(actualColumn, ordinalAsInt);
                        }

                    }
                }

            }
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
                throw new SprocMapperException($"Type mismatch for column {member.Name}. Expected type of {schemaProperty} but is instead of type {member.Type}");
            }

            if (schemaProperty != member.Type && nullableType == null)
            {
                throw new SprocMapperException($"Type mismatch for column {member.Name}. Expected type of {schemaProperty} but is instead of type {member.Type}");
            }
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
    }
}