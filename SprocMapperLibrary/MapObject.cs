using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;

namespace SprocMapperLibrary
{
    public class MapObject<T>
    {
        private HashSet<string> Columns { get; set; }
        private Dictionary<string, string> CustomColumnMappings { get; set; }

        public MapObject(SprocMapper sprocMapper)
        {
            CustomColumnMappings = new Dictionary<string, string>();
            Columns = new HashSet<string>();
        }

        public SprocAddColumn<T> AddColumn(Expression<Func<T, object>> columnName)
        {
            var propertyName = SprocMapperHelper.GetPropertyName(columnName);
            Columns.Add(propertyName);
            return new SprocAddColumn<T>(Columns, CustomColumnMappings);
        }

        public SprocAddColumn<T> AddColumn(Expression<Func<T, object>> columnName, string destination)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            var propertyName = SprocMapperHelper.GetPropertyName(columnName);
            Columns.Add(propertyName);

            CustomColumnMappings.Add(propertyName, destination);

            return new SprocAddColumn<T>(Columns, CustomColumnMappings);
        }

        /// <summary>
        /// Adds all properties in model that are either value, string, char[] or byte[] type. 
        /// </summary>
        /// <returns></returns>
        public SprocAddColumnList<T> AddAllColumns()
        {
            Columns = SprocMapperHelper.GetAllValueTypeAndStringColumns(typeof(T));
            return new SprocAddColumnList<T>(Columns, CustomColumnMappings);
        }
    }


}
