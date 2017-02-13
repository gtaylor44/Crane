using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SprocMapperLibrary
{
    public static class PropertyMapper
    {
        public static MapObject<T> MapObject<T>()
        {
            return new MapObject<T>();
        }
    }
    public static class SprocMapperExtensions
    {
        public static Select Select(this IDbConnection conn)
        {
            return new Select();
        }
    }
}
