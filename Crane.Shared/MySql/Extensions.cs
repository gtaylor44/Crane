using MySql.Data.MySqlClient;

namespace Crane.MySql
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
            var defaultOptions = new QueryOptions
            {
                CacheProvider = null,
                ValidateSelectColumns = false
            };

            return new MySqlUserQuery(sqlConnection.ConnectionString, defaultOptions);
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
