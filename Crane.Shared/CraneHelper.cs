using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
#if NETFRAMEWORK
using Microsoft.SqlServer.Types;
#endif
using System.Reflection;

[assembly: InternalsVisibleTo("UnitTest")]
[assembly: InternalsVisibleTo("IntegrationTest")]
namespace Crane
{
    internal static class CraneHelper
    {
        public static void SetOrdinal(List<DataRow> rowList, List<ICraneObjectMap> sprocObjectMapList, int[] partitionOnOrdinal)
        {
            if (rowList == null)
                throw new CraneException("Could not get schema for stored procedure");

            int currMap = 0;

            foreach (var map in sprocObjectMapList)
            {

                foreach (var column in map.Columns)
                {
                    string actualColumn = column;

                    if (map.CustomColumnMappings.ContainsKey(actualColumn))
                        actualColumn = map.CustomColumnMappings[actualColumn];

                    if (map.ColumnOrdinalDic.ContainsKey(actualColumn))
                        throw new CraneException($"Could not assign ordinal for column {actualColumn}");

                    int? pos;
                    if (partitionOnOrdinal != null)
                    {
                        pos = currMap == sprocObjectMapList.Count - 1 ? GetOrdinalPosition(rowList, actualColumn, partitionOnOrdinal[currMap],
                                null) : GetOrdinalPosition(rowList, actualColumn, partitionOnOrdinal[currMap],
                                partitionOnOrdinal[currMap + 1]);
                    }
                    else
                    {
                        pos = GetOrdinalPosition(rowList, actualColumn, 0, null);
                    }

                    if (pos.HasValue)
                    {
                        map.ColumnOrdinalDic.Add(actualColumn, pos.Value);
                    }
                }

                currMap++;
            }
        }

        public static int? GetOrdinalPosition(List<DataRow> dataRowList, string columnName, int minRange, int? maxRange)
        {
            foreach (var dataRow in dataRowList)
            {
                int ordinalAsInt = int.Parse(dataRow["ColumnOrdinal"].ToString());

                if (maxRange.HasValue)
                {
                    if (dataRow["ColumnName"].ToString().Equals(columnName, StringComparison.OrdinalIgnoreCase)
                        && ordinalAsInt >= minRange
                        && ordinalAsInt < maxRange)
                    {
                        return ordinalAsInt;
                    }
                }
                else if (dataRow["ColumnName"].ToString().Equals(columnName, StringComparison.OrdinalIgnoreCase)
                         && ordinalAsInt >= minRange)
                {
                    return ordinalAsInt;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the ordinal as a start index for each column in partitionOn string. 
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="partitionOnArr"></param>
        /// <param name="mapCount"></param>
        /// <returns></returns>
        public static int[] GetOrdinalPartition(List<DataRow> rows, string[] partitionOnArr, int mapCount)
        {

            List<int> result = new List<int>();
            List<string> matched = new List<string>();

            if (partitionOnArr.Length != mapCount)
                throw new CraneException($"Invalid number of arguments entered for partitionOn. Expected {mapCount} arguments but instead saw {partitionOnArr.Length} arguments");

            int currPartition = 0;

            for (int i = 0; i < rows.Count; i++)
            {
                string selectParam = rows[i]["ColumnName"].ToString();

                if (i == 0 && !string.Equals(selectParam, partitionOnArr[currPartition], StringComparison.OrdinalIgnoreCase))
                {
                    throw new CraneException($"First partitionOn argument is incorrect. Expected {selectParam} but instead saw {partitionOnArr[currPartition]}");
                }

                if (string.Equals(selectParam, partitionOnArr[currPartition], StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(int.Parse(rows[i]["ColumnOrdinal"].ToString()));
                    matched.Add(selectParam);
                    currPartition++;
                }

                if (currPartition == partitionOnArr.Length)
                    break;
            }

            if (currPartition != partitionOnArr.Length)
            {
                string matchedStr = string.Join(", ", matched);
                throw new CraneException($"Please check that partitionOn arguments are all valid column names. Crane was only able to match the following arguments: {matchedStr}. Expecting a total of {mapCount} valid arguments.");
            }

            return result.ToArray();

        }

        public static void ValidatePartitionOn(string partitionOn)
        {
            if (partitionOn == null)
                throw new ArgumentNullException(nameof(partitionOn));

            var validPattern = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_ ]+(?:\|[a-zA-Z_][a-zA-Z0-9_ ]*)*$");

            if (!validPattern.IsMatch(partitionOn))
                throw new CraneException("partitionOn pattern is incorrect. Must be letters or digits and separated by a pipe. E.g. 'OrderId|ProductId'");
        }

        public static bool ValidateSelectColumns(List<DataRow> rows, List<ICraneObjectMap> sprocObjectMapList,
            int[] partitionOnOrdinal, string storedProcedure)
        {
            List<string> absentColumnMessageList = new List<string>();

            if (sprocObjectMapList.Count == 1)
            {
                ICraneObjectMap objectMap = sprocObjectMapList.ElementAt(0);
                foreach (var row in rows)
                {
                    string currColumn = row["ColumnName"].ToString();

                    if (!objectMap.ColumnOrdinalDic.ContainsKey(currColumn))
                        absentColumnMessageList.Add(
                            $"Select column: '{currColumn}'\nTarget model: '{objectMap.Type.Name}'");
                }
            }

            else
            {
                int currMap = 0;

                foreach (var map in sprocObjectMapList)
                {
                    int minRange = partitionOnOrdinal[currMap];
                    int maxRange = (sprocObjectMapList.Count - 1) == currMap ? rows.Count : partitionOnOrdinal[currMap + 1];

                    for (int i = minRange; i < maxRange; i++)
                    {
                        string currColumn = rows[i]["ColumnName"].ToString();
                        if (!map.ColumnOrdinalDic.ContainsKey(currColumn))
                            absentColumnMessageList.Add($"Select column: '{currColumn}'\nTarget model: '{map.Type.Name}'");

                    }

                    currMap++;
                }
            }

            string absentColumnsAsString = string.Join(",\n\n", absentColumnMessageList);

            string message = sprocObjectMapList.Count == 1 ? $"The following columns from the select statement in '{storedProcedure}' have not been " +
                                                             $"mapped to target model '{sprocObjectMapList.ElementAt(0).Type.Name}'.\n\n{absentColumnsAsString}\n" :
                                                             $"The following columns from select statement have not been mapped to target model. " +
                                                             $"The target model is determined by the 'partitionOn' parameter. This validation message is dependant on a sound partitionOn argument. \n\n{absentColumnsAsString}\n";

            if (absentColumnMessageList.Count > 0)
                throw new CraneException($"'validateSelectColumns' flag is set to TRUE\n\n{message}");

            return true;
        }

        public static string GetActualColumn(string columnName, ICraneObjectMap objectMap)
        {
            return objectMap.CustomColumnMappings.ContainsKey(columnName)
                ? objectMap.CustomColumnMappings[columnName]
                : columnName;
        }

        public static bool ValidateCustomColumnMappings(List<ICraneObjectMap> sprocObjectMapList)
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

        public static void ValidateSchema(DataTable schema, List<ICraneObjectMap> sprocObjectMapList, int[] partitionOnOrdinal)
        {
            var rows = schema?.Rows.Cast<DataRow>().ToList();

            if (rows == null)
                return;

            if (sprocObjectMapList.Count == 1)
            {
                ICraneObjectMap map = sprocObjectMapList.ElementAt(0);
                foreach (var row in rows)
                {
                    string currColumn = row["ColumnName"].ToString();

                    map.Columns.ToList().ForEach(x =>
                    {
                        if (map.CustomColumnMappings.ContainsKey(x))
                        {
                            if (map.CustomColumnMappings[x].Equals(currColumn,
                                StringComparison.Ordinal))
                            {
                                ValidateColumn(map, x, row);
                            }
                        }
                        else if (x.Equals(currColumn,
                            StringComparison.Ordinal))
                        {
                            ValidateColumn(map, x, row);

                        }
                    });
                }
            }

            else
            {
                int currMap = 0;

                foreach (var map in sprocObjectMapList)
                {
                    int minRange = partitionOnOrdinal[currMap];
                    int maxRange = (sprocObjectMapList.Count - 1) == currMap ? rows.Count : partitionOnOrdinal[currMap + 1];

                    for (int i = minRange; i < maxRange; i++)
                    {
                        string currColumn = rows[i]["ColumnName"].ToString();

                        map.Columns.ToList().ForEach(x =>
                        {
                            if (map.CustomColumnMappings.ContainsKey(x))
                            {
                                if (map.CustomColumnMappings[x].Equals(currColumn,
                                    StringComparison.Ordinal))
                                {
                                    ValidateColumn(map, x, rows[i]);
                                }
                            }
                            else if (x.Equals(currColumn,
                                StringComparison.Ordinal))
                            {
                                ValidateColumn(map, x, rows[i]);

                            }
                        });

                    }

                    currMap++;
                }
            }
        }

        public static void ValidateColumn(ICraneObjectMap map, string schemaColumn, DataRow occurence)
        {
            PropertyInfo member;

            if (!map.PropertyInfoCache.TryGetValue(schemaColumn, out member))
            {
                throw new KeyNotFoundException($"Could not get schema property {schemaColumn}");
            }

            var schemaProperty = (Type)occurence["DataType"];

            var type = member.PropertyType;

            if (member.PropertyType.BaseType == typeof(Enum) 
                && (schemaProperty == typeof(int) || schemaProperty == typeof(long)))
                return;

            Type nullableType;
            if (((nullableType = Nullable.GetUnderlyingType(type)) != null && schemaProperty != nullableType)
                || schemaProperty != type && nullableType == null)
            {
                throw new CraneException($"Type mismatch for column '{member.Name}'. Expected type of '{schemaProperty}' but instead saw type '{type}'");
            }
        }

        public static void MapObject<T>(List<ICraneObjectMap> sprocObjectMapList, Dictionary<Type, Dictionary<string, string>> customColumnMappings)
        {
            CraneObjectMap<T> objectMap = new CraneObjectMap<T>();

            objectMap.Columns = GetAllValueTypeAndStringColumns(objectMap);

            Dictionary<string, string> customColumnDic;

            if (customColumnMappings.TryGetValue(typeof(T), out customColumnDic))
            {
                objectMap.CustomColumnMappings = customColumnDic;
            }

            sprocObjectMapList.Add(objectMap);
        }

        public static HashSet<string> GetAllValueTypeAndStringColumns<TObj>(CraneObjectMap<TObj> mapObject)
        {
            HashSet<string> columns = new HashSet<string>();

            //Get all properties

            var members = typeof(TObj).GetRuntimeProperties();

            foreach (var member in members)
            {
                if (CheckForValidDataType(member.PropertyType))
                {
                    columns.Add(member.Name);
                    mapObject.PropertyInfoCache.Add(member.Name, member);
                    mapObject.TypeInfoCache.Add(member.Name, member.PropertyType.GetTypeInfo());
                }
            }

            return columns;

        }

        public static Dictionary<int, string> GetColumnsForDynamicQuery(DataTable schema)
        {
            var rowList = schema?.Rows.Cast<DataRow>().ToList();
            Dictionary<int, string> dynamicColumnDic = new Dictionary<int, string>();

            if (rowList != null)
                foreach (DataRow dr in rowList)
                {
                    int ordinalAsInt = int.Parse(dr["ColumnOrdinal"].ToString());
                    string strippedColumnName = dr["ColumnName"]?.ToString().Replace(" ", string.Empty);

                    if (strippedColumnName == null)
                        throw new CraneException("There was a probelm retrieving column.");

                    if (dynamicColumnDic.ContainsValue(strippedColumnName))
                        throw new CraneException($"Duplicate column name in stored procedure detected: {strippedColumnName}");

                    dynamicColumnDic.Add(ordinalAsInt, strippedColumnName);
                }

            return dynamicColumnDic;
        }

        /// <summary>
        /// Resolves an object of type T and handles DBNull.Value gracefully.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sprocObjectMap"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static T GetObject<T>(ICraneObjectMap sprocObjectMap, IDataReader reader)
        {
            var targetObject = (T)Activator.CreateInstance(typeof(T));
            int defaultOrNullCounter = 0;

            foreach (var column in sprocObjectMap.Columns)
            {
                var actualColumn = sprocObjectMap.CustomColumnMappings.ContainsKey(column)
                    ? sprocObjectMap.CustomColumnMappings[column] : column;

                int ordinal;
                if (!sprocObjectMap.ColumnOrdinalDic.TryGetValue(actualColumn, out ordinal))
                {
                    defaultOrNullCounter++;
                    continue;
                }


                PropertyInfo member;
                if (!sprocObjectMap.PropertyInfoCache.TryGetValue(column, out member))
                {
                    throw new KeyNotFoundException($"Could not get property for column {column}");
                }

                object readerObj = reader[ordinal];

                if (readerObj == DBNull.Value)
                {
                    var isNullable = Nullable.GetUnderlyingType(member.PropertyType) != null;

                    if (member.PropertyType.IsValueType && !isNullable)
                        member.SetValue(targetObject, Convert.ChangeType(GetDefaultValue(member, sprocObjectMap.DefaultValueDic), member.PropertyType), null);

                    defaultOrNullCounter++;
                }

                else
                {
                    var t = member.PropertyType;

                    if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                    {
                        t = Nullable.GetUnderlyingType(t);
                    }

                    if (t.BaseType == typeof(Enum))
                    {
                        member.SetValue(targetObject, (int)readerObj, null);
                    }

                    else
                    {
                        member.SetValue(targetObject, Convert.ChangeType(readerObj, t), null);
                    }
                    
                }
            }

            if (defaultOrNullCounter == sprocObjectMap.Columns.Count)
                return default(T); // All columns are null or default

            return targetObject;
        }

        /// <summary>
        /// Gets the default value of type. Caches the default value for performance.
        /// </summary>
        /// <param name="member"></param>
        /// <param name="defaultValueDic"></param>
        /// <returns></returns>
        public static object GetDefaultValue(PropertyInfo member, Dictionary<string, object> defaultValueDic)
        {
            object obj;
            if (defaultValueDic.TryGetValue(member.Name, out obj))
            {
                return obj;
            }
            obj = Activator.CreateInstance(member.PropertyType);
            defaultValueDic.Add(member.Name, obj);
            return obj;
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
#if NETFRAMEWORK
                ||
                type == typeof(SqlGeography) ||
                type == typeof(SqlGeometry)
#endif
                )
                return true;

            return false;
        }
    }
}
