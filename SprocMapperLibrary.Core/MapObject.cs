using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FastMember;

namespace SprocMapperLibrary.Core
{    
    public class MapObject<T>
    {
        internal HashSet<string> Columns { get; set; }
        internal Dictionary<string, string> CustomColumnMappings { get; set; }
        internal Dictionary<string, Member> MemberInfoCache { get; set; }
        internal TypeAccessor TypeAccessor { get; set; }

        public MapObject()
        {
            CustomColumnMappings = new Dictionary<string, string>();
            Columns = new HashSet<string>();
            MemberInfoCache = new Dictionary<string, Member>();
        }

        /// <summary>
        /// Adds all properties in model that are either value, string, char[] or byte[] type. 
        /// </summary>
        /// <returns></returns>
        public MapObject<T> AddAllColumns()
        {
            Columns = GetAllValueTypeAndStringColumns(this);
            return this;
        }

        public MapObject<T> CustomColumnMapping(Expression<Func<T, object>> source, string destination)
        {
            var propertyName = SprocMapper.GetPropertyName(source);

            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            var typeAccessor = TypeAccessor.Create(typeof(T));

            //Get all properties
            MemberSet members = typeAccessor.GetMembers();

            foreach (var member in members)
            {
                if (member.Name.Equals(destination, StringComparison.OrdinalIgnoreCase))
                    throw new SprocMapperException($"Custom column mapping must map to a unique " +
                                                   $"property. A property with the name '{destination}' already exists.");
            }

            CustomColumnMappings.Add(propertyName, destination);
            return this;
        }

        public SprocObjectMap<T> GetMap()
        {
            return new SprocObjectMap<T>()
            {
                Columns = Columns,
                CustomColumnMappings = CustomColumnMappings,
                MemberInfoCache = MemberInfoCache,
                TypeAccessor = TypeAccessor
            };
        }

        public static HashSet<string> GetAllValueTypeAndStringColumns<TObj>(MapObject<TObj> mapObject)
        {
            HashSet<string> columns = new HashSet<string>();

            var typeAccessor = TypeAccessor.Create(typeof(T));
            mapObject.TypeAccessor = typeAccessor;

            //Get all properties
            MemberSet members = typeAccessor.GetMembers();

            foreach (var member in members)
            {
                if (SprocMapper.CheckForValidDataType(member.Type))
                {
                    columns.Add(member.Name);
                    mapObject.MemberInfoCache.Add(member.Name, member);
                }
            }

            return columns;

        }
    }


}
