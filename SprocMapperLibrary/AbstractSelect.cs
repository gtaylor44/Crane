using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;

namespace SprocMapperLibrary
{
    public abstract class AbstractSelect
    {
        protected List<SqlParameter> ParamList;
        protected List<ISprocObjectMap> SprocObjectMapList;

        protected AbstractSelect(List<ISprocObjectMap> sprocObjectMapList)
        {
            ParamList = new List<SqlParameter>();
            SprocObjectMapList = sprocObjectMapList;
        }

        protected void AddSqlParameterList(List<SqlParameter> paramList)
        {
            if (paramList == null)
                // ReSharper disable once NotResolvedInText
                throw new ArgumentNullException("AddSqlParameterList does not accept null value");

            ParamList = paramList;
        }

        protected void ValidateProperties()
        {
            if (!SprocMapperHelper.ValidateProperies1(SprocObjectMapList))
            {
                throw new SprocMapperException($"Duplicate column not allowed. Ensure that all columns in stored procedure are unique." +
                                               "Try setting an alias for your column in your stored procedure " +
                                               "and set up a custom column mapping.");
            }
        }

        protected void OpenConn(SqlConnection conn)
        {
            if (conn.State == ConnectionState.Closed)
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

        protected void RemoveAbsentColumns(SqlDataReader reader)
        {
            HashSet<string> columns = new HashSet<string>(StringComparer.Ordinal);
            DataTable schema = reader.GetSchemaTable();

            var occurrences1 = schema?.Rows.Cast<DataRow>();

            if (occurrences1 != null)
            {
                foreach (var occurence in occurrences1)
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
                    }
                    else if (!columns.Contains(x))
                    {

                        map.Columns.Remove(x);
                    }
                });
            }
        }

        protected void ValidateSchema(SqlDataReader reader)
        {
            // Get the schema which represents the columns in the reader
            DataTable schema = reader.GetSchemaTable();

            // Find all columns in the schema which match the name we're looking for.
            // schema is a table and each row is a column from our reader.
            var occurrences = schema?.Rows.Cast<DataRow>().Where(r => string.Equals((string)r["ColumnName"], "IsHonest", StringComparison.Ordinal));

            // Get the nthOccurrence.  Will throw if occurrences is empty.
            // reader.GetOrdinal will also throw if a column is not present, but you may want to
            // have this throw a more meaningful exception
            var occurrence = occurrences?.Skip(1 - 1).FirstOrDefault();

            if (occurrence != null)
            {

                // return the ordinal
                int test = (int)occurrence["ColumnOrdinal"];

                Type type = (Type)occurrence["DataType"];
            }

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
                                    ValidateColumn(map, schemaColumn, occurence);                                 
                                }
                            }
                            else if (x.Equals(schemaColumn,
                                StringComparison.Ordinal))
                            {
                                ValidateColumn(map, schemaColumn, occurence);

                            }
                        });
                    }
                }
            }
        }

        private void ValidateColumn(ISprocObjectMap map, string schemaColumn, DataRow occurence)
        {
            var property = map.Type.GetProperty(schemaColumn);

            if (property != null)
            {

                var schemaProperty = (Type)occurence["DataType"];

                Type nullableType;
                if ((nullableType = Nullable.GetUnderlyingType(property.PropertyType)) != null && schemaProperty != nullableType)
                {
                    throw new SprocMapperException($"Type mismatch for column {property.Name}. Expected type of {schemaProperty} but is instead of type {property.PropertyType}");
                }

                if (schemaProperty != property.PropertyType && nullableType == null)
                {
                    throw new SprocMapperException($"Type mismatch for column {property.Name}. Expected type of {schemaProperty} but is instead of type {property.PropertyType}");
                }
            }
        }

    }
}
