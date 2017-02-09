namespace SprocMapperLibrary
{
    public class SprocMapper
    {
        public MapObject<T> MapObject<T>()
        {
            return new MapObject<T>(this);
        }

        public Select Select(SprocObjectMap objectMap)
        {
            return new Select(objectMap);
        }

    }

    public class SprocMapper<T>
    {

    }
}
