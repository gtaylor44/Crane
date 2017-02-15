using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using FastMember;
using SprocMapperLibrary.CustomException;

[assembly: InternalsVisibleTo("IntegrationTest")]
[assembly: InternalsVisibleTo("UnitTest")]
namespace SprocMapperLibrary
{    
    public class MapObject<T>
    {
        internal HashSet<string> Columns { get; set; }
        internal HashSet<string> IgnoreColumns { get; set; }
        internal Dictionary<string, string> CustomColumnMappings { get; set; }
        internal Dictionary<string, Member> MemberInfoCache { get; set; }
        internal TypeAccessor TypeAccessor { get; set; }

        public MapObject()
        {
            CustomColumnMappings = new Dictionary<string, string>();
            Columns = new HashSet<string>();
            IgnoreColumns = new HashSet<string>();
            MemberInfoCache = new Dictionary<string, Member>();
        }

        /// <summary>
        /// Adds all properties in model that are either value, string, char[] or byte[] type. 
        /// </summary>
        /// <returns></returns>
        internal MapObject<T> AddAllColumns()
        {
            Columns = GetAllValueTypeAndStringColumns(this);
            return this;
        }

        public MapObject<T> IgnoreColumn(Expression<Func<T, object>> source)
        {
            var propertyName = GetPropertyName(source);
            IgnoreColumns.Add(propertyName);
            return this;
        }

        public MapObject<T> CustomColumnMapping(Expression<Func<T, object>> source, string destination)
        {
            var propertyName = GetPropertyName(source);
            CustomColumnMappings.Add(propertyName, destination);
            return this;
        }

        internal SprocObjectMap<T> GetMap()
        {
            return new SprocObjectMap<T>()
            {
                Columns = Columns,
                CustomColumnMappings = CustomColumnMappings,
                MemberInfoCache = MemberInfoCache,
                TypeAccessor = TypeAccessor
            };
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
