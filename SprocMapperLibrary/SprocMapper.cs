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
        public static Select<T> Select<T>(this SqlConnection conn)
        {
            List<ISprocObjectMap> objectMapList = new List<ISprocObjectMap>();

            var objectMap = PropertyMapper.MapObject<T>()
                .AddAllColumns()
                .GetMap();

            objectMapList.Add(objectMap);

            return new Select<T>(objectMapList);
        }

        public static Select<T> Select<T>(this SqlConnection conn, MapObject<T> objectMap)
        {
            List<ISprocObjectMap> objectMapList = new List<ISprocObjectMap>();

            MapObject(objectMap, objectMapList);

            return new Select<T>(objectMapList);
        }

        public static Select2<T> Select<T, T1>(this SqlConnection conn, MapObject<T> objectMap = null, MapObject<T1> objectMap2 = null)
        {
            List<ISprocObjectMap> objectMapList = new List<ISprocObjectMap>();

            MapObject(objectMap, objectMapList);
            MapObject(objectMap2, objectMapList);

            return new Select2<T>(objectMapList);
        }

        public static Select3<T> Select<T, T1, T2>(this SqlConnection conn, MapObject<T> objectMap = null, MapObject<T1> objectMap2 = null, 
            MapObject<T2> objectMap3 = null)
        {
            List<ISprocObjectMap> objectMapList = new List<ISprocObjectMap>();

            MapObject(objectMap, objectMapList);
            MapObject(objectMap2, objectMapList);
            MapObject(objectMap3, objectMapList);

            return new Select3<T>(objectMapList);
        }

        public static Select4<T> Select<T, T1, T2, T3>(this SqlConnection conn, MapObject<T> objectMap = null, MapObject<T1> objectMap2 = null, 
            MapObject<T2> objectMap3 = null, MapObject<T3> objectMap4 = null)
        {
            List<ISprocObjectMap> objectMapList = new List<ISprocObjectMap>();

            MapObject(objectMap, objectMapList);
            MapObject(objectMap2, objectMapList);
            MapObject(objectMap3, objectMapList);
            MapObject(objectMap4, objectMapList);

            return new Select4<T>(objectMapList);
        }

        public static Select5<T> Select<T, T1, T2, T3, T4>(this SqlConnection conn, MapObject<T> objectMap = null, MapObject<T1> objectMap2 = null, 
            MapObject<T2> objectMap3 = null, MapObject<T3> objectMap4 = null, MapObject<T4> objectMap5 = null)
        {
            List<ISprocObjectMap> objectMapList = new List<ISprocObjectMap>();

            MapObject(objectMap, objectMapList);
            MapObject(objectMap2, objectMapList);
            MapObject(objectMap3, objectMapList);
            MapObject(objectMap4, objectMapList);
            MapObject(objectMap5, objectMapList);

            return new Select5<T>(objectMapList);
        }

        public static Select6<T> Select<T, T1, T2, T3, T4, T5>(this SqlConnection conn, MapObject<T> objectMap = null, 
            MapObject<T1> objectMap2 = null, MapObject<T2> objectMap3 = null, MapObject<T3> objectMap4 = null, MapObject<T4> objectMap5 = null, 
            MapObject<T5> objectMap6 = null)
        {
            List<ISprocObjectMap> objectMapList = new List<ISprocObjectMap>();

            MapObject(objectMap, objectMapList);
            MapObject(objectMap2, objectMapList);
            MapObject(objectMap3, objectMapList);
            MapObject(objectMap4, objectMapList);
            MapObject(objectMap5, objectMapList);
            MapObject(objectMap6, objectMapList);

            return new Select6<T>(objectMapList);
        }

        public static Select7<T> Select<T, T1, T2, T3, T4, T5, T6>(this SqlConnection conn, MapObject<T> objectMap = null, 
            MapObject<T1> objectMap2 = null, MapObject<T2> objectMap3 = null, MapObject<T3> objectMap4 = null, MapObject<T4> objectMap5 = null, 
            MapObject<T5> objectMap6 = null, MapObject<T6> objectMap7 = null)
        {
            List<ISprocObjectMap> objectMapList = new List<ISprocObjectMap>();

            MapObject(objectMap, objectMapList);
            MapObject(objectMap2, objectMapList);
            MapObject(objectMap3, objectMapList);
            MapObject(objectMap4, objectMapList);
            MapObject(objectMap5, objectMapList);
            MapObject(objectMap6, objectMapList);
            MapObject(objectMap7, objectMapList);

            return new Select7<T>(objectMapList);
        }

        public static Select8<T> Select<T, T1, T2, T3, T4, T5, T6, T7>(this SqlConnection conn, MapObject<T> objectMap = null, 
            MapObject<T1> objectMap2 = null, MapObject<T2> objectMap3 = null, MapObject<T3> objectMap4 = null, MapObject<T4> objectMap5 = null, 
            MapObject<T5> objectMap6 = null, MapObject<T6> objectMap7 = null, MapObject<T7> objectMap8 = null)
        {
            List<ISprocObjectMap> objectMapList = new List<ISprocObjectMap>();

            MapObject(objectMap, objectMapList);
            MapObject(objectMap2, objectMapList);
            MapObject(objectMap3, objectMapList);
            MapObject(objectMap4, objectMapList);
            MapObject(objectMap5, objectMapList);
            MapObject(objectMap6, objectMapList);
            MapObject(objectMap7, objectMapList);
            MapObject(objectMap8, objectMapList);

            return new Select8<T>(objectMapList);
        }

        public static Select9<T> Select<T, T1, T2, T3, T4, T5, T6, T7, T8>(this SqlConnection conn, MapObject<T> objectMap = null, 
            MapObject<T1> objectMap2 = null, MapObject<T2> objectMap3 = null, MapObject<T3> objectMap4 = null, MapObject<T4> objectMap5 = null,
            MapObject<T5> objectMap6 = null, MapObject<T6> objectMap7 = null, MapObject<T7> objectMap8 = null, MapObject<T8> objectMap9 = null)
        {
            List<ISprocObjectMap> objectMapList = new List<ISprocObjectMap>();

            MapObject(objectMap, objectMapList);
            MapObject(objectMap2, objectMapList);
            MapObject(objectMap3, objectMapList);
            MapObject(objectMap4, objectMapList);
            MapObject(objectMap5, objectMapList);
            MapObject(objectMap6, objectMapList);
            MapObject(objectMap7, objectMapList);
            MapObject(objectMap8, objectMapList);
            MapObject(objectMap9, objectMapList);

            return new Select9<T>(objectMapList);
        }

        public static Select10<T> Select<T, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this SqlConnection conn, MapObject<T> objectMap = null, 
            MapObject<T1> objectMap2 = null, MapObject<T2> objectMap3 = null, MapObject<T3> objectMap4 = null, MapObject<T4> objectMap5 = null, 
            MapObject<T5> objectMap6 = null, MapObject<T6> objectMap7 = null, MapObject<T7> objectMap8 = null, MapObject<T8> objectMap9 = null, 
            MapObject<T9> objectMap10 = null)
        {
            List<ISprocObjectMap> objectMapList = new List<ISprocObjectMap>();

            MapObject(objectMap, objectMapList);
            MapObject(objectMap2, objectMapList);
            MapObject(objectMap3, objectMapList);
            MapObject(objectMap4, objectMapList);
            MapObject(objectMap5, objectMapList);
            MapObject(objectMap6, objectMapList);
            MapObject(objectMap7, objectMapList);
            MapObject(objectMap8, objectMapList);
            MapObject(objectMap9, objectMapList);
            MapObject(objectMap10, objectMapList);

            return new Select10<T>(objectMapList);
        }

        public static Select11<T> Select<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this SqlConnection conn, MapObject<T> objectMap = null, 
            MapObject<T1> objectMap2 = null, MapObject<T2> objectMap3 = null, MapObject<T3> objectMap4 = null, MapObject<T4> objectMap5 = null, 
            MapObject<T5> objectMap6 = null, MapObject<T6> objectMap7 = null, MapObject<T7> objectMap8 = null, MapObject<T8> objectMap9 = null, 
            MapObject<T9> objectMap10 = null, MapObject<T10> objectMap11 = null)
        {
            List<ISprocObjectMap> objectMapList = new List<ISprocObjectMap>();

            MapObject(objectMap, objectMapList);
            MapObject(objectMap2, objectMapList);
            MapObject(objectMap3, objectMapList);
            MapObject(objectMap4, objectMapList);
            MapObject(objectMap5, objectMapList);
            MapObject(objectMap6, objectMapList);
            MapObject(objectMap7, objectMapList);
            MapObject(objectMap8, objectMapList);
            MapObject(objectMap9, objectMapList);
            MapObject(objectMap10, objectMapList);
            MapObject(objectMap11, objectMapList);

            return new Select11<T>(objectMapList);
        }

        public static Select12<T> Select<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this SqlConnection conn, MapObject<T> objectMap = null, 
            MapObject<T1> objectMap2 = null, MapObject<T2> objectMap3 = null, MapObject<T3> objectMap4 = null, MapObject<T4> objectMap5 = null, 
            MapObject<T5> objectMap6 = null, MapObject<T6> objectMap7 = null, MapObject<T7> objectMap8 = null, MapObject<T8> objectMap9 = null, 
            MapObject<T9> objectMap10 = null, MapObject<T10> objectMap11 = null, MapObject<T11> objectMap12 = null)
        {
            List<ISprocObjectMap> objectMapList = new List<ISprocObjectMap>();

            MapObject(objectMap, objectMapList);
            MapObject(objectMap2, objectMapList);
            MapObject(objectMap3, objectMapList);
            MapObject(objectMap4, objectMapList);
            MapObject(objectMap5, objectMapList);
            MapObject(objectMap6, objectMapList);
            MapObject(objectMap7, objectMapList);
            MapObject(objectMap8, objectMapList);
            MapObject(objectMap9, objectMapList);
            MapObject(objectMap10, objectMapList);
            MapObject(objectMap11, objectMapList);
            MapObject(objectMap12, objectMapList);

            return new Select12<T>(objectMapList);
        }

        private static void MapObject<T>(MapObject<T> map, List<ISprocObjectMap> mapList)
        {
            if (map == null)
                map = PropertyMapper.MapObject<T>();

            map.AddAllColumns();

            mapList.Add(map.GetMap());
        }
    }
}
