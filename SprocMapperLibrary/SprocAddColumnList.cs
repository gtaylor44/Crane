using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SprocMapperLibrary
{
    public class SprocAddColumnList<T>
    {
        private readonly HashSet<string> _columns;
        private readonly Dictionary<string, string> _customColumnMappings;
        public SprocAddColumnList(HashSet<string> columns, Dictionary<string, string> customColumnMappings)
        {
            _columns = columns;
            _customColumnMappings = customColumnMappings;
        }

        public SprocAddColumnList<T> CustomColumnMapping(Expression<Func<T, object>> source, string destination)
        {
            var propertyName = SprocMapperHelper.GetPropertyName(source);
            _customColumnMappings.Add(propertyName, destination);
            return this;
        }
        public SprocAddColumnList<T> RemoveColumn(Expression<Func<T, object>> columnName)
        {
            var propertyName = SprocMapperHelper.GetPropertyName(columnName);
            if (_columns.Contains(propertyName))
                _columns.Remove(propertyName);

            else
                throw new SprocMapperException("Could not remove the column with name "
                    + columnName +
                    ". This could be because it's not a value or string type and therefore not included.");

            return this;
        }

        public SprocObjectMap GetMap()
        {
            return new SprocObjectMap()
            {
                Columns = _columns,
                CustomColumnMappings = _customColumnMappings
            };
        }
    }
}
