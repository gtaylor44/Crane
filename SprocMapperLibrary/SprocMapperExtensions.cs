using System.Data;

namespace SprocMapperLibrary
{
    public static class SprocMapperExtensions
    {
        public static Select Select(this IDbConnection conn)
        {
            return new Select();
        }

        public static Procedure Procedure(this IDbConnection conn)
        {
            return new Procedure();
        }
    }
}
