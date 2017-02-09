using System.Collections.Generic;
using System.Configuration;
using System.Transactions;
using Model;
using SqlBulkTools;
using System.Data.SqlClient;

namespace IntegrationTest.Initialise
{
    public class Seed
    {
        private static List<President> GetPresidentList()
        {
            List<President> presidents = new List<President>();

            presidents.AddPresident("Donald", "Trump", 100000000);
            presidents.AddPresident("George", "Washington", 234334);
            presidents.AddPresident("Thomas", "Jefferson", 65456564);
            presidents.AddPresident("George", "Bush", 654654);
            presidents.AddPresident("Richard", "Nixon", 654654);

            return presidents;
        }

        private static List<PresidentAssistant> GetPresidentAssistantList()
        {
            List<PresidentAssistant> presAssistant = new List<PresidentAssistant>();

            presAssistant.AddPresidentAssistant("Greg", "Taylor");
            presAssistant.AddPresidentAssistant("Ezra", "Taylor");
            presAssistant.AddPresidentAssistant("Dorian", "Broadrick");
            presAssistant.AddPresidentAssistant("Elena", "Broadrick");

            return presAssistant;
        }

        public static void InsertOrUpdateData()
        {
            BulkOperations bulk = new BulkOperations();

            List<President> presidents = GetPresidentList();
            List<PresidentAssistant> presAssistants = GetPresidentAssistantList();

            using (TransactionScope transaction = new TransactionScope())
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SprocMapperTest"].ConnectionString))
                {
                    bulk.Setup<President>()
                        .ForCollection(presidents)
                        .WithTable("President")
                        .AddAllColumns()
                        .BulkInsertOrUpdate()
                        .MatchTargetOn(x => x.Id)
                        .Commit(conn);

                    bulk.Setup<PresidentAssistant>()
                        .ForCollection(presAssistants)
                        .WithTable("PresidentAssistant")
                        .AddAllColumns()
                        .BulkInsertOrUpdate()
                        .MatchTargetOn(x => x.Id)
                        .Commit(conn);
                }

                transaction.Complete();
            }
        }
    }

    public static class SeedExtension
    {
        public static void AddPresident(this List<President> presidentList, string firstName, string lastName, int? fans)
        {
            int listCount = presidentList.Count;
            presidentList.Add(new President()
            {
                Id = listCount + 1,
                FirstName = firstName,
                LastName = lastName,
                Fans = fans
            });
        }

        public static void AddPresidentAssistant(this List<PresidentAssistant> presidentAssistantList, string firstName, string lastName)
        {
            int listCount = presidentAssistantList.Count;
            presidentAssistantList.Add(new PresidentAssistant()
            {
                Id = listCount + 1,
                PresidentId = 1,
                FirstName = firstName,
                LastName = lastName,
            });
        }
    }
}
