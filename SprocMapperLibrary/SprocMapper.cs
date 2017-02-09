using System;

namespace SprocMapperLibrary
{

    public class SprocMapper<T>
    {
        private Type _type;
        public MapObject<T> MapObject()
        {
            _type = typeof(T);
            return new MapObject<T>(_type);
        }

        public Select<T> Select(SprocObjectMap<T> objectMap)
        {
            return new Select<T>(objectMap);
        }
    }
}
