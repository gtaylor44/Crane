using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SprocMapperLibrary
{
    public class Select<T> : AbstractSelect
    {
        public Select(List<ISprocObjectMap> sprocObjectMapList) : base(sprocObjectMapList){ }

        public new Select<T> AddSqlParameterList(List<SqlParameter> paramList)
        {
            base.AddSqlParameterList(paramList);
            return this;
        }

        public List<T> ExecuteReader(SqlConnection conn, string procName, int commandTimeout = 600)
        {
            ValidateProperties();
            OpenConn(conn);

            List<T> result = new List<T>();


            using (SqlCommand command = new SqlCommand(procName, conn))
            {
                SetCommandProps(command, commandTimeout);

                var reader = command.ExecuteReader();

                ValidateSchema(reader);
                RemoveAbsentColumns(reader);

                if (!reader.HasRows)
                    return default(List<T>);

                while (reader.Read())
                {
                    T obj = SprocMapperHelper.GetObject<T>(SprocObjectMapList[0].Columns,
                        SprocObjectMapList[0].CustomColumnMappings, reader);
                    result.Add(obj);
                }

            }
            return result;

        }

        public List<string> GetAbsentProperties(SqlConnection conn, string procName)
        {
            OpenConn(conn);
            using (SqlCommand command = new SqlCommand(procName, conn))
            {

                //var reader = command.ExecuteReader();



                command.CommandType = CommandType.StoredProcedure;


                var reader = command.ExecuteReader(CommandBehavior.KeyInfo);

                ValidateProperties();
                RemoveAbsentColumns(reader);

                //Retrieve column schema into a DataTable.
                var schema = reader.GetSchemaTable();

                HashSet<string> missingColumnSet = new HashSet<string>();

                var occurrences1 = schema?.Rows.Cast<DataRow>();

                if (occurrences1 != null)
                {
                    HashSet<string> mappedColumnSet = new HashSet<string>();

                    foreach (var occurence in occurrences1)
                    {
                        string schemaColumn = (string)occurence["ColumnName"];

                        foreach (ISprocObjectMap map in SprocObjectMapList)
                        {
                            
                        }
                    }
                }


                return null;
            }
        }

    }

}

