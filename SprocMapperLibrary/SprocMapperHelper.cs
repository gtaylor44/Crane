using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
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
                var actualColumn = sprocObjectMap.CustomColumnMappings.ContainsKey(column)
                    ? sprocObjectMap.CustomColumnMappings[column] : column;

                Member member;
                if (!sprocObjectMap.MemberInfoCache.TryGetValue(column, out member))
                {
                    throw new KeyNotFoundException($"Could not get property for column {column}");
                }


                object readerObj = reader[actualColumn];

                if (readerObj == DBNull.Value)
                {
                    sprocObjectMap.TypeAccessor[targetObject, member.Name] = GetDefaultValue(member);
                }

                else
                {
                    sprocObjectMap.TypeAccessor[targetObject, member.Name] = readerObj;
                }

            }

            return targetObject;
        }

        static object GetDefaultValue(Member member)
        {
            if (member.Type.IsValueType)
                return Activator.CreateInstance(member.Type);
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

        internal static HashSet<string> GetAllValueTypeAndStringColumns<T>(MapObject<T> mapObject)
        {
            HashSet<string> columns = new HashSet<string>();

            var typeAccessor = TypeAccessor.Create(typeof(T));
            mapObject.TypeAccessor = typeAccessor;

            //Get all properties
            MemberSet members = typeAccessor.GetMembers();

            foreach (var member in members)
            {
                if (CheckForValidDataType(member.Type) && !mapObject.IgnoreColumns.Contains(member.Name))
                {
                    columns.Add(member.Name);
                    mapObject.MemberInfoCache.Add(member.Name, member);
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
