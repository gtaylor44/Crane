using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq.Expressions;
using System.Reflection;
using FastMember;

namespace SprocMapperLibrary
{
    public static class SprocMapperHelper
    {
        public static T GetObject<T>(ISprocObjectMap sprocObjectMap, IDataReader reader) 
        {
            T targetObject = NewInstance<T>.Instance();

            foreach (var column in sprocObjectMap.Columns)
            {
                string actualColumn;
                if (sprocObjectMap.CustomColumnMappings.ContainsKey(column))
                {
                    actualColumn = sprocObjectMap.CustomColumnMappings[column];
                }
                else
                {
                    actualColumn = column;
                }

                PropertyInfo prop;
                if (!sprocObjectMap.PropertyInfoCache.TryGetValue(column, out prop))
                {
                    PropertyInfo propInfo = targetObject.GetType().GetProperty(column);
                    sprocObjectMap.PropertyInfoCache.Add(column, propInfo);

                    prop = propInfo;
                }

                if (prop != null)
                {
                    object readerObj = reader[actualColumn];

                    if (readerObj == DBNull.Value)
                    {
                        var accessor = TypeAccessor.Create(targetObject.GetType());
                        accessor[targetObject, prop.Name] = GetDefaultValue(prop);
                    }

                    else
                    {
                        var accessor = TypeAccessor.Create(targetObject.GetType());
                        accessor[targetObject, prop.Name] = readerObj;
                    }
                }
            }

            return targetObject;
        }

        static object GetDefaultValue(PropertyInfo prop)
        {
            if (prop.PropertyType.IsValueType)
                return Activator.CreateInstance(prop.PropertyType);
            return null;
        }

        public static Dictionary<int, string> GetColumnIndex(Type type, int startIndex)
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();
            foreach (var property in type.GetProperties())
            {
                dic.Add(startIndex++, property.Name);
            }

            return dic;
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

        internal static HashSet<string> GetAllValueTypeAndStringColumns(Type type, HashSet<string> ignoreColumns)
        {
            HashSet<string> columns = new HashSet<string>();

            //Get all the properties
            PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < props.Length; i++)
            {
                if (CheckForValidDataType(props[i].PropertyType) && !ignoreColumns.Contains(props[i].Name))
                {
                    columns.Add(props[i].Name);
                }
            }

            return columns;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="throwIfInvalid">
        /// Set this to true if user is manually adding columns. If AddAllColumns is used, then this can be omitted. 
        /// </param>
        /// <returns></returns>
        private static bool CheckForValidDataType(Type type, bool throwIfInvalid = false)
        {
            if (type.IsValueType ||
                type == typeof(string) ||
                type == typeof(byte[]) ||
                type == typeof(char[]) ||
                type == typeof(SqlXml)
                )
                return true;

            if (throwIfInvalid)
                throw new SprocMapperException($"Only value, string, char[], byte[] and SqlXml types can be used " +
                                                $"with SqlBulkTools. Refer to https://msdn.microsoft.com/en-us/library/cc716729(v=vs.110).aspx for " +
                                                $"more details.");

            return false;
        }



    }

}
