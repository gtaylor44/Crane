using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntegrationTest.Initialise;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model;
using SprocMapperLibrary;

namespace IntegrationTest
{
    [TestClass]
    public class SelectAsyncTest
    {
        [TestInitialize]
        public void Setup()
        {
            Seed.InsertOrUpdateData();
        }

        [TestMethod]
        public async Task SelectSingleTable()
        {
            using (SqlConnection conn =
                new SqlConnection(ConfigurationManager.ConnectionStrings["SprocMapperTest"].ConnectionString))
            {
                var result = await conn.Select()
                    .ExecuteReaderAsync<President>(conn, "dbo.GetPresidentList2");

                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        public async Task SelectWithJoin()
        {
            using (SqlConnection conn =
                new SqlConnection(ConfigurationManager.ConnectionStrings["SprocMapperTest"].ConnectionString))
            {
                Dictionary<int, President> dic = new Dictionary<int, President>();

                await conn.Select()
                    .AddMapping(PropertyMapper
                            .MapObject<PresidentAssistant>()
                            .CustomColumnMapping(x => x.Id, "Assistant Id")
                            .CustomColumnMapping(x => x.FirstName, "Assistant First Name")
                            .CustomColumnMapping(x => x.LastName, "Assistant Last Name"))
                    .ExecuteReaderAsync<President, PresidentAssistant>(conn, "dbo.GetPresidentList", (p, pa) =>
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
                    });

                Assert.IsNotNull(dic.Values);
            }
        }
    }
}
