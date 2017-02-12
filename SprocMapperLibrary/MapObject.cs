using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using FastMember;

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
            Columns = SprocMapperHelper.GetAllValueTypeAndStringColumns(this);
            return this;
        }

        public MapObject<T> IgnoreColumn(Expression<Func<T, object>> source)
        {
            var propertyName = SprocMapperHelper.GetPropertyName(source);
            IgnoreColumns.Add(propertyName);
            return this;
        }

        public MapObject<T> CustomColumnMapping(Expression<Func<T, object>> source, string destination)
        {
            var propertyName = SprocMapperHelper.GetPropertyName(source);
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
    }


}
