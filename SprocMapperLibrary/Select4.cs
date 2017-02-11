﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
namespace SprocMapperLibrary
{
    public class Select4<T> : AbstractSelect
    {
        public Select4(List<ISprocObjectMap> sprocObjectMapList) : base(sprocObjectMapList){}

        public new Select4<T> AddSqlParameterList(IEnumerable<SqlParameter> paramList)
        {
            base.AddSqlParameterList(paramList);
            return this;
        }

        public List<T> ExecuteReader<T1, T2, T3, T4>(SqlConnection conn, string procName, Func<T1, T2, T3, T4, T> callBack,
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

                if (!reader.HasRows)
                    return default(List<T>);

                while (reader.Read())
                {
                    T1 obj1 = SprocMapperHelper.GetObject<T1>(SprocObjectMapList[0].Columns,
                        SprocObjectMapList[0].CustomColumnMappings, reader);
                    T2 obj2 = SprocMapperHelper.GetObject<T2>(SprocObjectMapList[1].Columns,
                        SprocObjectMapList[1].CustomColumnMappings, reader);
                    T3 obj3 = SprocMapperHelper.GetObject<T3>(SprocObjectMapList[2].Columns,
                        SprocObjectMapList[2].CustomColumnMappings, reader);
                    T4 obj4 = SprocMapperHelper.GetObject<T4>(SprocObjectMapList[3].Columns,
                        SprocObjectMapList[3].CustomColumnMappings, reader);

                    T obj = callBack.Invoke(obj1, obj2, obj3, obj4);

                    result.Add(obj);
                }

            }
            return result;
        }
    }

}

