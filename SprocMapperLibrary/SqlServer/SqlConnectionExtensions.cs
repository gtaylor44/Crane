using System.Data;
using System.Data.SqlClient;

namespace SprocMapperLibrary.SqlServer
{
    /// <summary>
    /// 
    /// </summary>
    public static class SqlConnectionExtensions
    {
        /// <summary>
        /// Entry point for performing a select.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static SqlServerSelect Select(this SqlConnection conn)
        {
            return new SqlServerSelect(conn);
        }

        /// <summary>
        /// Entry point for performing a Sql Server procedure.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static Procedure Procedure(this SqlConnection conn)
        {
            return new Procedure(conn);
        }
    }
}
