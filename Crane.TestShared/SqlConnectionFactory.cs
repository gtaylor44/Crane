using System.Data.SqlClient;

namespace Crane.TestCommon
{
    public static class SqlConnectionFactory
    {

        public static string SqlConnectionString => @"Data Source=THINKPAD\SQLSERVER;Initial Catalog=SprocMapperTest;Integrated Security=True;Pooling=false;";
        //public static string SqlConnectionString => "Data Source=DESKTOP-6I9FL7M;Initial Catalog=SprocMapperTest;Integrated Security=True;Pooling=false;";
    }
}
