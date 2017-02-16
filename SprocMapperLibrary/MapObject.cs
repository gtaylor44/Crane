using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using FastMember;

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
            var propertyName = SprocMapper.GetPropertyName(source);
            IgnoreColumns.Add(propertyName);
            return this;
        }

        public MapObject<T> CustomColumnMapping(Expression<Func<T, object>> source, string destination)
        {
            var propertyName = SprocMapper.GetPropertyName(source);
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

        internal static HashSet<string> GetAllValueTypeAndStringColumns<TObj>(MapObject<TObj> mapObject)
        {
            HashSet<string> columns = new HashSet<string>();

            var typeAccessor = TypeAccessor.Create(typeof(T));
            mapObject.TypeAccessor = typeAccessor;

            //Get all properties
            MemberSet members = typeAccessor.GetMembers();

            foreach (var member in members)
            {
                if (SprocMapper.CheckForValidDataType(member.Type) && !mapObject.IgnoreColumns.Contains(member.Name))
                {
                    columns.Add(member.Name);
                    mapObject.MemberInfoCache.Add(member.Name, member);
                }
            }

            return columns;

        }
    }


}
