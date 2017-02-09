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
            SprocMapper sprocMapper = new SprocMapper();

            SqlConnection conn =
                new SqlConnection(ConfigurationManager.ConnectionStrings["SprocMapperTest"].ConnectionString);

            var objectMapping = sprocMapper
                .MapObject<President>()
                .AddAllColumns()
                .GetMap();

            var objectMapping1 = sprocMapper.MapObject<PresidentAssistant>()
                .AddAllColumns()
                .RemoveColumn(x => x.Id)
                .CustomColumnMapping(x => x.FirstName, "AssistantFirstName")
                .CustomColumnMapping(x => x.LastName, "AssistantLastName")
                .GetMap();

            var result = sprocMapper
                .Select(objectMapping)
                .Join("Id", objectMapping1)
                .ExecuteReaderWithJoin<President, PresidentAssistant>(conn, "dbo.GetPresidentList");

            Assert.AreEqual(5, result.Count);

        }
    }
}
