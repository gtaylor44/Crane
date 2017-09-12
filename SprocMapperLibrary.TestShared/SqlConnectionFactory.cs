using System.Data.SqlClient;

namespace SprocMapperLibrary.TestCommon
{
    public static class SqlConnectionFactory
    {
        public static SqlConnection GetSqlConnection()
        {
            return new SqlConnection("Data Source=DESKTOP-6I9FL7M;Initial Catalog=SprocMapperTest;Integrated Security=True;Pooling=false;");
        }

        public static string SqlConnectionString => "Data Source=DESKTOP-6I9FL7M;Initial Catalog=SprocMapperTest;Integrated Security=True;Pooling=false;";
    }
}
