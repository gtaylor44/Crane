using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace SprocMapperLibrary.SqlServer
{
    /// <summary>
    /// 
    /// </summary>
    public static class SqlConnectionExtensions
    {
        /// <summary>
        /// Entry point for performing a Sql Server procedure.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static SqlServerProcedure Procedure(this SqlConnection conn)
        {
            return new SqlServerProcedure(conn);
        }
    }
}
