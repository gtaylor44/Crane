using System.Collections.Generic;
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
        public static Select<T> Select<T>(this SqlConnection conn, SprocObjectMap<T> objectMap)
        {
            List<ISprocObjectMap> objectMapList = new List<ISprocObjectMap>();

            objectMapList.AddRange(new List<ISprocObjectMap>() { objectMap });

            return new Select<T>(objectMapList);
        }

        public static Select2<T> Select<T, T1>(this SqlConnection conn, SprocObjectMap<T> objectMap, SprocObjectMap<T1> objectMap2)
        {
            List<ISprocObjectMap> objectMapList = new List<ISprocObjectMap>();
            objectMapList.AddRange(new List<ISprocObjectMap>() { objectMap , objectMap2});

            return new Select2<T>(objectMapList);
        }

        public static Select3<T> Select<T, T1>(this SqlConnection conn, SprocObjectMap<T> objectMap, SprocObjectMap<T1> objectMap2, SprocObjectMap<T1> objectMap3)
        {
            List<ISprocObjectMap> objectMapList = new List<ISprocObjectMap>();
            objectMapList.AddRange(new List<ISprocObjectMap>() { objectMap, objectMap2 });

            return new Select3<T>(objectMapList);
        }
        public static Select<T> Select<T>(this SqlConnection conn)
        {
            List<ISprocObjectMap> objectMapList = new List<ISprocObjectMap>();

            var objectMap = PropertyMapper.MapObject<T>()
                .AddAllColumns()
                .GetMap();

            objectMapList.Add(objectMap);

            return new Select<T>(objectMapList);
        }
    }
}
