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
                    .ExecuteReader(conn, "dbo.GetPresidentList2");

                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        public void TestMethod3()
        {
            using (SqlConnection conn =
                new SqlConnection(ConfigurationManager.ConnectionStrings["SprocMapperTest"].ConnectionString))
            {
                Dictionary<int,President> dic = new Dictionary<int, President>();

                conn
                    .Select<President, PresidentAssistant>(

                    null,

                    PropertyMapper
                    .MapObject<PresidentAssistant>()     
                    //.CustomColumnMapping(x => x.Id, "Assistant Id")               
                    .CustomColumnMapping(x => x.FirstName, "Assistant First Name")
                    .CustomColumnMapping(x => x.LastName, "Assistant Last Name"))

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
