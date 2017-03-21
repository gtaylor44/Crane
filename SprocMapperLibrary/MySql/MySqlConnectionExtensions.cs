using MySql.Data.MySqlClient;

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
        public static MySqlSproc Sproc(this MySqlConnection conn)
        {
            return new MySqlSproc(conn);
        }
    }
}
