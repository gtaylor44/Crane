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
            SprocMapper<President> presidentMapping = new SprocMapper<President>();
            SprocMapper<President> presidentAssistantMapping = new SprocMapper<President>();

            SqlConnection conn =
                new SqlConnection(ConfigurationManager.ConnectionStrings["SprocMapperTest"].ConnectionString);

            var objectMapping = presidentMapping
                .MapObject()
                .AddAllColumns()
                .GetMap();

            var objectMapping1 = presidentAssistantMapping.MapObject()
                .AddAllColumns()
                .RemoveColumn(x => x.Id)
                .CustomColumnMapping(x => x.FirstName, "AssistantFirstName")
                .CustomColumnMapping(x => x.LastName, "AssistantLastName")
                .GetMap();

            var result = presidentMapping
                .Select(objectMapping)
                .JoinMany<PresidentAssistant>(x => x.Id, x => x.PresidentAssistantList, objectMapping1)
                .ExecuteReaderWithJoin<President, PresidentAssistant>(conn, "dbo.GetPresidentList");

            Assert.AreEqual(5, result.Count);

        }
    }
}
