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
        /// Entry point for performing a MySql procedure.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static MySqlSproc Procedure(this MySqlConnection conn)
        {
            return new MySqlSproc(conn);
        }
    }
}
