﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;

namespace SprocMapperLibrary
{
    public class MapObject<T>
    {
        private HashSet<string> Columns { get; set; }
        private HashSet<string> IgnoreColumns { get; set; }
        private Dictionary<string, string> CustomColumnMappings { get; set; }
        public MapObject()
        {
            CustomColumnMappings = new Dictionary<string, string>();
            Columns = new HashSet<string>();
            IgnoreColumns = new HashSet<string>();
        }

        //public MapObject(HashSet<string> columns, Dictionary<string, string> customColumnMappings)
        //{
        //    CustomColumnMappings = customColumnMappings;
        //    Columns = columns;
        //}

        /// <summary>
        /// Adds all properties in model that are either value, string, char[] or byte[] type. 
        /// </summary>
        /// <returns></returns>
        internal MapObject<T> AddAllColumns()
        {
            Columns = SprocMapperHelper.GetAllValueTypeAndStringColumns(typeof(T), IgnoreColumns);
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
                CustomColumnMappings = CustomColumnMappings
            };
        }
    }


}
