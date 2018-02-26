using Crane.Shared.Base;
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
            var defaultOptions = new QueryOptions
            {
                ValidateSelectColumns = false
            };

#if NETFRAMEWORK
            return new SqlServerQuery(sqlConnection.ConnectionString, sqlConnection.Credential, defaultOptions);
#endif
#if NETCORE
            return new SqlServerQuery(sqlConnection.ConnectionString, defaultOptions);
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
