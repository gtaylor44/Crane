using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using FastMember;
using SprocMapperLibrary.CustomException;

namespace SprocMapperLibrary
{
    public static class SprocMapper
    {
        internal static bool ValidateDuplicateSelectAliases(DataTable schema, bool pedanticValidation, List<ISprocObjectMap> sprocObjectMapList)
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
                ClearUnusedMaps(columns, sprocObjectMapList);
            }

            return true;
        }

        internal static void ClearUnusedMaps(HashSet<string> columns, List<ISprocObjectMap> sprocObjectMapList)
        {
            HashSet<string> allColumns = new HashSet<string>();
            foreach (var map in sprocObjectMapList)
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

        internal static void ValidateSelectParams(HashSet<string> schemaColumnSet, HashSet<string> allColumns)
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
                                                   $"Ignore or set up a custom mapping for each model included in multi-map that has a property named '{key}'. The model that currently has this property mapped is " +
                                                   $"'{map.Type.Name}' You can ignore columns and setup custom mappings using the AddMapping method. See documentation for more info.\n");
                }
            }

            return true;
        }

        // Validate that no custom column mapping is mapped to 
        internal static bool ValidateCustomColumnMappings(List<ISprocObjectMap> sprocObjectMapList)
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


        internal static void ValidateSchema(DataTable schema, List<ISprocObjectMap> sprocObjectMapList)
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

        internal static void ValidateColumn(ISprocObjectMap map, string schemaColumn, DataRow occurence)
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

        internal static void MapObject<T, T1, T2, T3, T4, T5, T6, T7>(Dictionary<Type, ISprocObjectMap> sprocObjectMapDic, List<ISprocObjectMap> sprocObjectMapList)
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

        internal static void MapObject<T>(Dictionary<Type, ISprocObjectMap> sprocObjectMapDic, List<ISprocObjectMap> sprocObjectMapList)
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

        internal static T GetObject<T>(ISprocObjectMap sprocObjectMap, IDataReader reader)
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

        internal static object GetDefaultValue(Member member)
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

        internal static string GetPropertyName(Expression method)
        {
            LambdaExpression lambda = method as LambdaExpression;
            if (lambda == null)
                throw new ArgumentNullException("method");

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
                throw new ArgumentException("method");

            return memberExpr.Member.Name;
        }

        internal static bool CheckForValidDataType(Type type)
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
