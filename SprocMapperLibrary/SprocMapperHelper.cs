using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SprocMapperLibrary
{
    public static class SprocMapperHelper
    {
        public static T GetObject<T>(HashSet<string> columns, Dictionary<string, string> customColumnMappings, IDataReader reader) 
        {
            T targetObject = NewInstance<T>.Instance();

            //foreach (var prop in targetObject.GetType().GetProperties())
            //{
            //    var propTest = prop;
            //}

            foreach (var column in columns)
            {
                string actualColumn;
                if (customColumnMappings.ContainsKey(column))
                {
                    actualColumn = customColumnMappings[column];
                }
                else
                {
                    actualColumn = column;
                }

                Type objType = targetObject.GetType();
                PropertyInfo prop = objType.GetProperty(column);

               
                if (prop != null)
                {
                    object readerObj = reader[actualColumn];

                    if (readerObj == DBNull.Value)
                    {
                        prop.SetValue(targetObject, null, null);
                    }
                        
                    else
                        prop.SetValue(targetObject, readerObj, null);

                }
            }

            return targetObject;
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

        internal static HashSet<string> GetAllValueTypeAndStringColumns(Type type)
        {
            HashSet<string> columns = new HashSet<string>();

            //Get all the properties
            PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < props.Length; i++)
            {
                if (CheckForValidDataType(props[i].PropertyType))
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
        public static bool ValidateProperies(List<ISprocObjectMap> sprocObjectMapList)
        {
            List<string> allColumns = new List<string>();

            foreach (var map in sprocObjectMapList)
            {
                map.Columns.ToList().ForEach(x =>
                {
                    if (map.CustomColumnMappings.ContainsKey(x))
                    {
                        allColumns.Add(map.CustomColumnMappings[x]);
                        return;
                    }
                    allColumns.Add(x);
                });
            }

            int allColumnsCount = allColumns.GroupBy(x => x).Count();

            if (allColumnsCount != allColumns.Count)
            {
                return false;
            }

            return true;
        }

    }


}
