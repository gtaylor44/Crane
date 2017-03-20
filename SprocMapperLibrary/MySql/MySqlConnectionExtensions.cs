using System.Data;
using MySql.Data.MySqlClient;
using SprocMapperLibrary.SqlServer;

namespace SprocMapperLibrary.MySql
{
    /// <summary>
    /// 
    /// </summary>
    public static class SprocmapperExtensions
    {
        /// <summary>
        /// Entry point for performing a select.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static MySqlSelect Select(this MySqlConnection conn)
        {
            return new MySqlSelect(conn);
        }

        /// <summary>
        /// Entry point for performing a MySql procedure.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static Procedure Procedure(this MySqlConnection conn)
        {
            return new Procedure(conn);
        }
    }
}
