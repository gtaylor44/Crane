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
    public class Select
    {
        private List<SqlParameter> _paramList;
        private SprocObjectMap _sprocObjectMap;
        private List<SprocObjectMap> _joinList;

        public Select(SprocObjectMap sprocObjectMap)
        {
            _sprocObjectMap = sprocObjectMap;
            _paramList = new List<SqlParameter>();
            _joinList = new List<SprocObjectMap>();
        }

        public Select AddSqlParameterList(List<SqlParameter> paramList)
        {
            if (paramList == null)
                // ReSharper disable once NotResolvedInText
                throw new ArgumentNullException("AddSqlParameterList does not accept null");

            _paramList = paramList;
            return this;
        }

        public Select Join(string primaryKey, SprocObjectMap sprocObjectMap)
        {
            if (sprocObjectMap == null)
                // ReSharper disable once NotResolvedInText
                throw new ArgumentNullException("Join can't have null map");

            _joinList.Add(sprocObjectMap);
            return this;
        }

        public List<T> ExecuteReader<T>(SqlConnection sqlConnection, string cmdText, int commandTimeout = 600)
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

                    Dictionary<string, T> parentDic = new Dictionary<string, T>();

                    if (_joinList != null && _joinList.Any())
                    {
                        parentDic = new Dictionary<string, T>();
                    }

                    while (reader.Read())
                    {
                        T obj = SprocMapperHelper.GetObject<T>(_sprocObjectMap.Columns, _sprocObjectMap.CustomColumnMappings, reader);

                        if (_joinList != null && _joinList.Any())
                        {
                            parentDic.Add(obj.GetType().GetProperty("Id").GetValue(obj).ToString(), obj);
                        }



                        result.Add(obj);
                    }
                }
            }
            return result;
        }

        public List<T> ExecuteReaderWithJoin<T, T1>(SqlConnection sqlConnection, string cmdText, int commandTimeout = 600)
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
