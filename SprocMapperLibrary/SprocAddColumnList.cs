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
