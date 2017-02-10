using System.Data.SqlClient;

namespace SprocMapperLibrary
{

    public static class SprocMapper
    {
        public static MapObject<T> MapObject<T>()
        {
            return new MapObject<T>();
        }

        public static Select<T> Select<T>(SprocObjectMap<T> objectMap)
        {
            return new Select<T>(objectMap);
        }

        public static Select<T> Select<T>()
        {
            var objectMap = MapObject<T>()
                .AddAllColumns()
                .GetMap();

            return new Select<T>(objectMap);
        }
    }

    public static class SprocMapperExtensions
    {
        public static MapObject<T> MapObject<T>(this SqlConnection conn)
        {
            return new MapObject<T>();
        }

        public static Select<T> Select<T>(this SqlConnection conn, SprocObjectMap<T> objectMap)
        {
            return new Select<T>(objectMap);
        }
        public static Select<T> Select<T>(this SqlConnection conn)
        {
            var objectMap = conn.MapObject<T>()
                .AddAllColumns()
                .GetMap();

            return new Select<T>(objectMap);
        }
    }
}
