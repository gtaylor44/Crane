using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FastMember;

namespace SprocMapperLibrary
{
    public abstract class AbstractSelect
    {
        protected ICollection<SqlParameter> ParamList;
        protected List<ISprocObjectMap> SprocObjectMapList;

        protected AbstractSelect()
        {
            ParamList = new List<SqlParameter>();
        }

        protected void OpenConn(SqlConnection conn)
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
        }

        protected void SetCommandProps(SqlCommand command, int commandTimeout)
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = commandTimeout;

            if (ParamList != null && ParamList.Any())
                command.Parameters.AddRange(ParamList.ToArray());
        }

        protected void DoStrictValidation(DataTable schema)
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
                            $"Duplicate column not allowed. Ensure that all columns in stored procedure are unique." +
                            "Try setting an alias for your column in your stored procedure " +
                            "and set up a custom column mapping.");
                    }

                    columns.Add(schemaColumn);
                }
            }

            HashSet<string> allColumns = new HashSet<string>();
            foreach (var map in SprocObjectMapList)
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

        internal void SetOrdinal(DataTable schema, List<ISprocObjectMap> sprocObjectMapList)
        {
            var rowList = schema?.Rows.Cast<DataRow>().ToList();

            if (rowList == null)
                return;

            Dictionary<string, int> columnOccurenceDic = new Dictionary<string, int>();

            for (int i = 0; i < sprocObjectMapList.Count; i++)
            {

                foreach (var column in sprocObjectMapList[i].Columns)
                {
                    string actualColumn = column;

                    if (sprocObjectMapList[i].CustomColumnMappings.ContainsKey(actualColumn))
                        actualColumn = sprocObjectMapList[i].CustomColumnMappings[actualColumn];

                    int count;
                    if (!columnOccurenceDic.TryGetValue(actualColumn, out count))
                    {
                        columnOccurenceDic.Add(actualColumn, 1);
                    }
                    else
                    {
                        columnOccurenceDic[actualColumn] = ++count;
                    }

                    var occurrences = rowList.Where(r => string.Equals((string)r["ColumnName"], actualColumn, StringComparison.Ordinal));

                    int occurrenceCount = columnOccurenceDic[actualColumn];
                    var occurrence = occurrences.Skip(occurrenceCount - 1).FirstOrDefault();                  

                    if (occurrence != null)
                    {

                        int ordinalAsInt = int.Parse(occurrence["ColumnOrdinal"].ToString());
                        if (!sprocObjectMapList[i].ColumnOrdinalDic.ContainsKey(actualColumn))
                        {
                            sprocObjectMapList[i].ColumnOrdinalDic.Add(actualColumn, ordinalAsInt);
                        } 
                    }
                }

            }
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

        protected void ValidateSchema(DataTable schema)
        {
            var occurrences1 = schema?.Rows.Cast<DataRow>();

            if (occurrences1 != null)
            {
                foreach (var occurence in occurrences1)
                {
                    string schemaColumn = (string)occurence["ColumnName"];
                    foreach (var map in SprocObjectMapList)
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

        private static string GetPropertyValidationExceptionMessage(string propertyName, string className)
        {
            return
                $"Duplicate property detected before execution. The offending property is '{propertyName}' of class '{className}'. Either ignore this property or " +
                $"add a custom mapping explicitly. Refer to documentation if you need help http://github.com";
        }
    }
}
