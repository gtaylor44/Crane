using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SprocMapperLibrary
{
    public class Select<T>
    {
        private List<SqlParameter> _paramList;
        private SprocObjectMap<T> _sprocObjectMap;
        private List<Join> _joinManyList;
        private string _parentKey { get; set; }

        public Select(SprocObjectMap<T> sprocObjectMap)
        {
            _sprocObjectMap = sprocObjectMap;
            _paramList = new List<SqlParameter>();
            _joinManyList = new List<Join>();
        }

        public Select<T> AddSqlParameterList(List<SqlParameter> paramList)
        {
            if (paramList == null)
                // ReSharper disable once NotResolvedInText
                throw new ArgumentNullException("AddSqlParameterList does not accept null");

            _paramList = paramList;
            return this;
        }

        /// <summary>
        /// This method only needs to be called if JoinMany is being used.
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public Select<T> SetParentKey(Expression<Func<T, object>> parentId)
        {
            var parent = SprocMapperHelper.GetPropertyName(parentId);

            _parentKey = parent;
            return this;
        }

        public Select<T> JoinMany<T1>(SprocObjectMap<T1> sprocObjectMap, Expression<Func<T, object>> collection, Expression<Func<T1, object>> joinKey)
        {
            if (sprocObjectMap == null)
                // ReSharper disable once NotResolvedInText
                throw new ArgumentNullException("Join can't have null map");

            
            var collectionProperty = SprocMapperHelper.GetPropertyName(collection);
            var joinKeyProperty = SprocMapperHelper.GetPropertyName(joinKey);

            Join joinMany = new Join()
            {               
                Columns = sprocObjectMap.Columns,
                CustomColumnMappings = sprocObjectMap.CustomColumnMappings,
                JoinToList = collectionProperty,
                Type = typeof(T1),
                Key = joinKeyProperty
            };

            _joinManyList.Add(joinMany);
            return this;
        }

        public List<T> ExecuteReader(SqlConnection conn, string cmdText, int commandTimeout = 600)
        {
            List<string> allColumns = new List<string>();

            _sprocObjectMap.Columns.ToList().ForEach(x =>
            {
                if (_sprocObjectMap.CustomColumnMappings.ContainsKey(x))
                {
                    allColumns.Add(_sprocObjectMap.CustomColumnMappings[x]);
                    return;
                }
                allColumns.Add(x);
            });

            foreach (var join in _joinManyList)
            {
                join.Columns.ToList().ForEach(x =>
                {
                    if (join.CustomColumnMappings.ContainsKey(x))
                    {
                        allColumns.Add(join.CustomColumnMappings[x]);
                        return;
                    }
                    allColumns.Add(x);
                });
            }

            int allColumnsCount = allColumns.GroupBy(x => x).Count();

            if (allColumnsCount != allColumns.Count)
            {
                throw new SprocMapperException("Duplicate column not allowed. " +
                                               "Try setting an alias for your column in your stored procedure " +
                                               "and set up a custom column mapping for this property.");
            }

            if (_joinManyList.Count > 0)
            {
                return ExecuteReaderJoinMany(conn, cmdText, commandTimeout);
            }

            return ExecuteReaderFlat(conn, cmdText, commandTimeout);
            
        }

        public List<T> ExecuteReaderFlat(SqlConnection conn, string cmdText, int commandTimeout = 600)
        {
            List<T> result = new List<T>();

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
                    T obj = SprocMapperHelper.GetObject<T>(typeof(T), _sprocObjectMap.Columns, _sprocObjectMap.CustomColumnMappings, reader);
                    result.Add(obj);
                }

            }
            return result;
        }

        public List<T> ExecuteReaderJoinMany(SqlConnection conn, string cmdText, int commandTimeout = 600)
        {
            if (_parentKey == null)
                throw new SprocMapperException("Parent key can't be null when JoinMany invoked");

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

                    T obj = SprocMapperHelper.GetObject<T>(typeof(T), _sprocObjectMap.Columns, _sprocObjectMap.CustomColumnMappings, reader);
                    string key = obj.GetType().GetProperty(_parentKey).GetValue(obj).ToString();

                    T dicItem;

                    if (!parentDic.TryGetValue(key, out dicItem))
                    {
                        parentDic.Add(key, obj);

                        if (dicItem == null)
                            dicItem = obj;

                        foreach (var join in _joinManyList)
                        {
                            var prop = dicItem.GetType().GetProperty(join.JoinToList);
                            var listType = typeof(List<>);
                            var genericArgs = prop.PropertyType.GetGenericArguments();
                            var concreteType = listType.MakeGenericType(genericArgs);
                            var newList = Activator.CreateInstance(concreteType);
                            prop.SetValue(obj, newList);
                        }
                    }


                    foreach (var join in _joinManyList)
                    {
                        object test = SprocMapperHelper.GetObject<object>(join.Type, join.Columns, join.CustomColumnMappings, reader, join.Key);

                        if (test != null)
                        {
                            PropertyInfo childList = dicItem.GetType().GetProperty("PresidentAssistantList");

                            object childListVal = childList.GetValue(dicItem, null);
                            MethodInfo addMethod = dicItem.GetType().GetProperty("PresidentAssistantList").PropertyType.GetMethod("Add");
                            addMethod.Invoke(childListVal, new[] { (object)test });
                        }
                    }
                }
                return parentDic.Values.ToList();
            }
        }

    }
}
