using System;
using System.Configuration;
using System.Data.SqlClient;
using IntegrationTest.Initialise;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model;
using SprocMapperLibrary;

namespace IntegrationTest
{
    [TestClass]
    public class SelectTest
    {
        [TestInitialize]
        public void Setup()
        {
            Seed.InsertOrUpdateData();
        }

        [TestMethod]
        public void TestMethod1()
        {
            using (SqlConnection conn =
                new SqlConnection(ConfigurationManager.ConnectionStrings["SprocMapperTest"].ConnectionString))
            {
                var objectMapping = conn
                    .MapObject<President>()
                    .AddAllColumns()
                    .GetMap();

                var objectMapping1 = conn
                    .MapObject<PresidentAssistant>()
                    .AddAllColumns()
                    .CustomColumnMapping(x => x.Id, "AssistantId")
                    .CustomColumnMapping(x => x.FirstName, "AssistantFirstName")
                    .CustomColumnMapping(x => x.LastName, "AssistantLastName")
                    .GetMap();

                var result = conn
                    .Select(objectMapping)
                    .JoinMany(objectMapping1, x => x.PresidentAssistantList, x => x.PresidentId)
                    .SetParentKey(x => x.Id)
                    .ExecuteReader(conn, "dbo.GetPresidentList");

                Assert.AreEqual(5, result.Count);
            }
        }

        [TestMethod]
        public void TestMethod2()
        {
            using (SqlConnection conn =
                new SqlConnection(ConfigurationManager.ConnectionStrings["SprocMapperTest"].ConnectionString))
            {
                var result = conn
                    .Select<President>()
                    .ExecuteReader(conn, "dbo.GetPresidentList");

                Assert.IsNotNull(result);
            }
        }
    }
}
