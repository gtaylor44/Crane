using System.Data.SqlClient;

namespace Crane.SqlServer
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
        public static SqlServerQuery Query(this SqlConnection sqlConnection)
        {
#if NETFRAMEWORK
            return new SqlServerQuery(sqlConnection.ConnectionString, sqlConnection.Credential, null);
#elif NETCORE
            return new SqlServerQuery(sqlConnection.ConnectionString, null);
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <returns></returns>
        public static SqlServerCommand Command(this SqlConnection sqlConnection)
        {
            return new SqlServerCommand(sqlConnection.ConnectionString);
        }
    }
}
