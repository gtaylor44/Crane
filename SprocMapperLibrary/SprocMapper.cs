using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprocMapperLibrary
{
    public class SprocMapper
    {
        public MapObject<T> MapObject<T>()
        {
            return new MapObject<T>(this);
        }
    }

    public class SprocMapper<T>
    {
        public Select<T> Select(SprocObjectMap objectMap)
        {
            //if (typeof(T) != objectMap.GetType())
            //    throw new SprocMapperException("Object map is wrong type");

            return new Select<T>(objectMap);
        }
    }
}
