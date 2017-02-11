using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace SprocMapperLibrary
{

    public static class SprocMapperExtensions
    {
        public static MapObject<T> MapObject<T>(this SqlConnection conn)
        {
            return new MapObject<T>();
        }

        public static Select<T> Select<T>(this SqlConnection conn, SprocObjectMap<T> objectMap)
        {
            List<ISprocObjectMap> objectMapList = new List<ISprocObjectMap>();

            objectMapList.AddRange(new List<ISprocObjectMap>() { objectMap });

            return new Select<T>(objectMapList);
        }

        public static Select<T> Select<T, T1>(this SqlConnection conn, SprocObjectMap<T> objectMap, SprocObjectMap<T1> objectMap2)
        {
            List<ISprocObjectMap> objectMapList = new List<ISprocObjectMap>();

            objectMapList.AddRange(new List<ISprocObjectMap>() { objectMap , objectMap2});

            return new Select<T>(objectMapList);
        }
        public static Select<T> Select<T>(this SqlConnection conn)
        {
            List<ISprocObjectMap> objectMapList = new List<ISprocObjectMap>();

            var objectMap = conn.MapObject<T>()
                .AddAllColumns()
                .GetMap();

            objectMapList.Add(objectMap);

            return new Select<T>(objectMapList);
        }
    }
}
