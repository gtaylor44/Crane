using System.Configuration;
using System.Data.SqlClient;

namespace SprocMapperLibrary.TestCommon
{
    public static class SqlConnectionFactory
    {
        public static SqlConnection GetSqlConnection()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings["SprocMapperTest"].ConnectionString);
        }

        public static string SqlConnectionString => ConfigurationManager.ConnectionStrings["SprocMapperTest"]
            .ConnectionString;
    }
}
