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
    public class Select<T>
    {
        private List<SqlParameter> _paramList;
        private SprocObjectMap<T> _sprocObjectMap;
        private List<SprocObjectMap<T>> _joinList;

        public Select(SprocObjectMap<T> sprocObjectMap)
        {
            _sprocObjectMap = sprocObjectMap;
            _paramList = new List<SqlParameter>();
            _joinList = new List<SprocObjectMap<T>>();
        }

        public Select<T> AddSqlParameterList(List<SqlParameter> paramList)
        {
            if (paramList == null)
                // ReSharper disable once NotResolvedInText
                throw new ArgumentNullException("AddSqlParameterList does not accept null");

            _paramList = paramList;
            return this;
        }

        public Select<T> Join(Expression<Func<T, object>> columnName, SprocObjectMap<T> sprocObjectMap)
        {
            if (sprocObjectMap == null)
                // ReSharper disable once NotResolvedInText
                throw new ArgumentNullException("Join can't have null map");

            _joinList.Add(sprocObjectMap);
            return this;
        }

        public List<T> ExecuteReader(SqlConnection sqlConnection, string cmdText, int commandTimeout = 600)
        {
            List<T> result = new List<T>();
            using (SqlConnection conn = sqlConnection)
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                using (SqlCommand command = new SqlCommand(cmdText, conn))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = commandTimeout;

                    if (_paramList != null && _paramList.Any())
                        command.Parameters.AddRange(_paramList.ToArray());

                    var reader = command.ExecuteReader();

                    if (!reader.HasRows)
                        return default(List<T>);

                    while (reader.Read())
                    {
                        T obj = SprocMapperHelper.GetObject<T>(_sprocObjectMap.Columns, _sprocObjectMap.CustomColumnMappings, reader);
                        result.Add(obj);
                    }
                }
            }
            return result;
        }

        public List<T> ExecuteReaderWithJoin<T1>(SqlConnection sqlConnection,  string cmdText, int commandTimeout = 600)
        {
            using (SqlConnection conn = sqlConnection)
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                using (SqlCommand command = new SqlCommand(cmdText, conn))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = commandTimeout;

                    if (_paramList != null && _paramList.Any())
                        command.Parameters.AddRange(_paramList.ToArray());

                    var reader = command.ExecuteReader();

                    if (!reader.HasRows)
                        return default(List<T>);

                    Dictionary<string, T> parentDic = new Dictionary<string, T>();

                    while (reader.Read())
                    {

                        T obj = SprocMapperHelper.GetObject<T>(_sprocObjectMap.Columns, _sprocObjectMap.CustomColumnMappings, reader);
                        string key = obj.GetType().GetProperty("Id").GetValue(obj).ToString();

                        T dicItem;

                        if (!parentDic.TryGetValue(key, out dicItem))
                        {
                            parentDic.Add(key, obj);

                            if (dicItem == null)
                                dicItem = obj;
                        }

                        T1 test = SprocMapperHelper.GetObject<T1>(_joinList.First().Columns, _joinList.First().CustomColumnMappings, reader, "PresidentId");                       

                        if (test != null)
                        {
                            PropertyInfo childList = dicItem.GetType().GetProperty("PresidentAssistantList");

                            object childListVal = childList.GetValue(dicItem, null);
                            MethodInfo addMethod = dicItem.GetType().GetProperty("PresidentAssistantList").PropertyType.GetMethod("Add");
                            addMethod.Invoke(childListVal, new[] { (object)test });
                        }


                    }
                    return parentDic.Values.ToList();
                }

            }

        }

    }
}
