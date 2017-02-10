using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace SprocMapperLibrary
{
    public class SprocAddColumn<T>
    {
        private readonly HashSet<string> _columns;
        private readonly Dictionary<string, string> _customColumnMappings;
        public SprocAddColumn(HashSet<string> columns, Dictionary<string, string> customColumnMappings)
        {
            _columns = columns;
            _customColumnMappings = customColumnMappings;
        }

        public SprocAddColumn<T> AddColumn(Expression<Func<T, object>> columnName)
        {
            var propertyName = SprocMapperHelper.GetPropertyName(columnName);
            _columns.Add(propertyName);
            return this;
        }

        public SprocAddColumn<T> AddColumn(Expression<Func<T, object>> columnName, string destination)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            var propertyName = SprocMapperHelper.GetPropertyName(columnName);
            _columns.Add(propertyName);

            _customColumnMappings.Add(propertyName, destination);
            return this;
        }

        public SprocObjectMap<T> GetMap()
        {
            return new SprocObjectMap<T>()
            {
                Columns = _columns,
                CustomColumnMappings = _customColumnMappings
            };
        }
    }
}
