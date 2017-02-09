using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprocMapperLibrary
{
    public class Setup<T> where T : List<T>
    {
        public Setup()
        {
            
        }

        public Select<T> Select(SprocObjectMap objectMap)
        {
            if (typeof(T) != objectMap.GetType())
                throw new SprocMapperException("Object map is wrong type");

            return new Select<T>(objectMap);
        }
        
    }
}
