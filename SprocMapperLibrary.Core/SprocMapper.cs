using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using FastMember;

namespace SprocMapperLibrary.Core
{
    public static class SprocMapper
    {
        public static bool ValidateDuplicateSelectAliases(DataTable schema, List<ISprocObjectMap> sprocObjectMapList, string storedProcedure)
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

            return true;
        }

        public static void SetOrdinal(DataTable schema, List<ISprocObjectMap> sprocObjectMapList, string partitionOn)
        {
            var rowDic = schema?.Rows.Cast<DataRow>().ToDictionary(x => x["ColumnName"].ToString().ToLower());

            if (rowDic == null)
                return;

            int[] partitionOnOrdinal = null;

            if (partitionOn != null)
                partitionOnOrdinal = GetOrdinalPartition(schema, partitionOn, sprocObjectMapList.Count);

            int currMap = 0;

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

                        if (partitionOnOrdinal != null)
                        {
                            if (currMap == sprocObjectMapList.Count - 1 || 
                                (ordinalAsInt < partitionOnOrdinal[currMap + 1] && ordinalAsInt >= partitionOnOrdinal[currMap]))
                            {
                                if (map.ColumnOrdinalDic.ContainsKey(actualColumn))
                                    throw new SprocMapperException($"Could not assign ordinal for column {actualColumn}");

                                map.ColumnOrdinalDic.Add(actualColumn, ordinalAsInt);
                            }
                        }
                        else
                        {
                            map.ColumnOrdinalDic.Add(actualColumn, ordinalAsInt);
                        }
                    }
                }

                currMap++;
            }
        }

        /// <summary>
        /// Gets the ordinal as a start index for each column in partitionOn string. 
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="partitionOn"></param>
        /// <param name="mapCount"></param>
        /// <returns></returns>
        public static int[] GetOrdinalPartition(DataTable schema, string partitionOn, int mapCount)
        {

            List<int> result = new List<int>();
            string[] partitionOnArr = partitionOn.Split('|');

            if (partitionOnArr.Length != mapCount)
            {
                throw new SprocMapperException($"Invalid number of arguments entered for partitionOn. Expected {mapCount} arguments but instead saw {partitionOnArr.Length} arguments");
            }

            int currPartition = 0;

            var rows = schema?.Rows.Cast<DataRow>().ToList();

            for (int i = 0; i < rows.Count; i++)
            {
                string selectParam = rows[i]["ColumnName"].ToString();

                if (i == 0 && !string.Equals(selectParam, partitionOnArr[currPartition], StringComparison.OrdinalIgnoreCase))
                {
                    throw new SprocMapperException($"First partitionOn argument is incorrect. Expected {selectParam} but instead saw {partitionOnArr[currPartition]}");
                }

                if (string.Equals(selectParam, partitionOnArr[currPartition], StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(int.Parse(rows[i]["ColumnOrdinal"].ToString()));
                    currPartition++;
                }

                if (currPartition == partitionOnArr.Length)
                    break;
            }

            if (currPartition != partitionOnArr.Length)
            {
                throw new SprocMapperException($"Please check that partitionOn arguments are all valid column names. Was only able to match {currPartition} arguments");
            }

            return result.ToArray();

        }

        public static void ValidatePartitionOn(string partitionOn)
        {
            if (partitionOn == null)
                throw new ArgumentNullException(nameof(partitionOn));

            var validPattern = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]+(?:\|[a-zA-Z_][a-zA-Z0-9_]*)*$");

            if (!validPattern.IsMatch(partitionOn))
                throw new SprocMapperException("partitionOn pattern is incorrect. Must be letters or digits and separated by a pipe. E.g. 'Id|Id'");
        }

        public static bool ValidateCustomColumnMappings(List<ISprocObjectMap> sprocObjectMapList)
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

        public static void ValidateSchema(DataTable schema, List<ISprocObjectMap> sprocObjectMapList)
        {
            var dataRowLIst = schema?.Rows.Cast<DataRow>();

            if (dataRowLIst != null)
            {
                foreach (var occurence in dataRowLIst)
                {
                    string schemaColumn = (string)occurence["ColumnName"];
                    foreach (var map in sprocObjectMapList)
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

        public static void ValidateColumn(ISprocObjectMap map, string schemaColumn, DataRow occurence)
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

        public static void MapObject<T, T1, T2, T3, T4, T5, T6, T7>(Dictionary<Type, ISprocObjectMap> sprocObjectMapDic, List<ISprocObjectMap> sprocObjectMapList)
        {
            if (typeof(T) != typeof(NoMap))
                MapObject<T>(sprocObjectMapDic, sprocObjectMapList);

            if (typeof(T1) != typeof(NoMap))
                MapObject<T1>(sprocObjectMapDic, sprocObjectMapList);

            if (typeof(T2) != typeof(NoMap))
                MapObject<T2>(sprocObjectMapDic, sprocObjectMapList);

            if (typeof(T3) != typeof(NoMap))
                MapObject<T3>(sprocObjectMapDic, sprocObjectMapList);

            if (typeof(T4) != typeof(NoMap))
                MapObject<T4>(sprocObjectMapDic, sprocObjectMapList);

            if (typeof(T5) != typeof(NoMap))
                MapObject<T5>(sprocObjectMapDic, sprocObjectMapList);

            if (typeof(T6) != typeof(NoMap))
                MapObject<T6>(sprocObjectMapDic, sprocObjectMapList);

            if (typeof(T7) != typeof(NoMap))
                MapObject<T7>(sprocObjectMapDic, sprocObjectMapList);
        }

        public static void MapObject<T>(Dictionary<Type, ISprocObjectMap> sprocObjectMapDic, List<ISprocObjectMap> sprocObjectMapList)
        {
            ISprocObjectMap objMap;
            if (!sprocObjectMapDic.TryGetValue(typeof(T), out objMap))
            {
                var map = PropertyMapper.MapObject<T>();
                map.AddAllColumns();

                objMap = map.GetMap();
            }

            sprocObjectMapList.Add(objMap);
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

        public static object GetDefaultValue(Member member)
        {
            if (member.Type.IsValueType)
                return Activator.CreateInstance(member.Type);
            return null;
        }


        public static class NewInstance<T>
        {
            public static readonly Func<T> Instance =
                Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile();
        }

        public static string GetPropertyName(Expression method)
        {
            LambdaExpression lambda = method as LambdaExpression;
            if (lambda == null)
                throw new ArgumentNullException(nameof(method));

            MemberExpression memberExpr = null;

            if (lambda.Body.NodeType == ExpressionType.Convert)
            {
                memberExpr =
                    ((UnaryExpression)lambda.Body).Operand as MemberExpression;
            }
            else if (lambda.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpr = lambda.Body as MemberExpression;
            }

            if (memberExpr == null)
                throw new ArgumentException(nameof(method));

            return memberExpr.Member.Name;
        }

        public static bool CheckForValidDataType(Type type)
        {
            if (type.IsValueType ||
                type == typeof(string) ||
                type == typeof(byte[]) ||
                type == typeof(char[]) ||
                type == typeof(SqlXml)
                )
                return true;

            return false;
        }
    }
}
