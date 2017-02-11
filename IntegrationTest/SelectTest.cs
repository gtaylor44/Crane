using System.Collections.Generic;
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

        [TestMethod]
        public void TestMethod3()
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

                Dictionary<int,President> dic = new Dictionary<int, President>();

                conn
                    .Select(objectMapping, objectMapping1)
                    .ExecuteReader<President, PresidentAssistant>(conn, "dbo.GetPresidentList", (p, pa) =>
                    {
                        President president;
                        if (!dic.TryGetValue(p.Id, out president))
                        {
                            p.PresidentAssistantList = new List<PresidentAssistant>();
                            dic.Add(p.Id, p);                          
                        }

                        president = dic[p.Id];

                        if (pa.Id != default(int))
                        {
                            president.PresidentAssistantList.Add(pa);
                        }
                        return p;
                    });

                Assert.IsNotNull(dic.Values);
            }
        }
    }
}
