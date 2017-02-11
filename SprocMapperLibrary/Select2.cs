using System;
using System.Collections.Generic;
using System.Data.SqlClient;
namespace SprocMapperLibrary
{
    public class Select2<T> : AbstractSelect
    {
        public Select2(List<ISprocObjectMap> sprocObjectMapList) : base(sprocObjectMapList){}

        public new Select2<T> AddSqlParameterList(List<SqlParameter> paramList)
        {
            base.AddSqlParameterList(paramList);
            return this;
        }

        public List<T> ExecuteReader<T1, T2>(SqlConnection conn, string procName, Func<T1, T2, T> callBack,
            int commandTimeout = 600)
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
                    T1 obj1 = SprocMapperHelper.GetObject<T1>(SprocObjectMapList[0].Columns,
                        SprocObjectMapList[0].CustomColumnMappings, reader);
                    T2 obj2 = SprocMapperHelper.GetObject<T2>(SprocObjectMapList[1].Columns,
                        SprocObjectMapList[1].CustomColumnMappings, reader);

                    T obj = callBack.Invoke(obj1, obj2);

                    result.Add(obj);
                }

            }
            return result;
        }
    }

}

