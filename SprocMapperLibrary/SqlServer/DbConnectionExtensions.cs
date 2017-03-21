using System.Data.SqlClient;

namespace SprocMapperLibrary.SqlServer
{
    /// <summary>
    /// 
    /// </summary>
    public static class DbConnectionExtensions
    {
        /// <summary>
        /// Entry point for performing a Sql Server procedure.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static SqlServerSproc Sproc(this SqlConnection conn)
        {
            return new SqlServerSproc(conn);
        }
    }
}
