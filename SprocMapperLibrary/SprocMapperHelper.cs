using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Linq.Expressions;
using System.Reflection;

namespace SprocMapperLibrary
{
    public static class SprocMapperHelper
    {
        public static T GetObject<T>(HashSet<string> columns, object obj) 
        {
            foreach (var column in columns)
            {
                Type objType = obj.GetType();
                PropertyInfo prop = objType.GetProperty(column);

                SetProperty(objType, prop, obj);
                
            }

            return default(T);
        }

        public static void SetProperty(Type objType, PropertyInfo prop, object obj)
        {
            if (prop == null)
                return;

            if (prop.PropertyType == typeof(int))
            {
                SetInt(objType, prop, prop.GetValue(obj)?.ToString());
            }

            if (prop.PropertyType == typeof(int?))
            {
                SetInt(objType, prop, prop.GetValue(obj)?.ToString());
            }
        }

        public static void SetInt(Type objType, PropertyInfo prop, string valueToParse)
        {
            int value;

            if (int.TryParse(valueToParse, out value))
            {
                if (prop.CanWrite)
                {
                    prop.SetValue(objType, value);
                }
            }

    
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
    }
}
