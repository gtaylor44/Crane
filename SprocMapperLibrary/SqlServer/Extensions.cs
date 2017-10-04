using System.Data.SqlClient;

namespace SprocMapperLibrary.SqlServer
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
            return new SqlServerQuery(sqlConnection.ConnectionString, sqlConnection.Credential, null);
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
