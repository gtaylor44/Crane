using MySql.Data.MySqlClient;

namespace SprocMapperLibrary.MySql
{
    /// <summary>
    /// 
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <returns></returns>
        public static MySqlUserQuery Query(this MySqlConnection sqlConnection)
        {
            return new MySqlUserQuery(sqlConnection.ConnectionString, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <returns></returns>
        public static MySqlUserCommand Command(this MySqlConnection sqlConnection)
        {
            return new MySqlUserCommand(sqlConnection.ConnectionString);
        }
    }
}
