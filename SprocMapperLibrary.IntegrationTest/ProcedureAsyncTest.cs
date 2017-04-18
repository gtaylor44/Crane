using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SprocMapperLibrary;
using SprocMapperLibrary.SqlServer;
using SprocMapperLibrary.TestCommon;
using SprocMapperLibrary.TestCommon.Model;

namespace IntegrationTest
{
    [TestClass]
    public class ProcedureAsyncTest
    {
        [TestMethod]
        public async Task InsertCustomerThenDelete()
        {
            SqlServerAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);

            Customer customer = new Customer()
            {
                City = "Auckland",
                Country = "New Zealand",
                FirstName = "Greg",
                LastName = "Taylor",
                Phone = "021222222"
            };

            int inserted = 0;


            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                using (SqlConnection conn = SqlConnectionFactory.GetSqlConnection())
                {
                    conn.Open();
                    SqlParameter idParam = new SqlParameter() { ParameterName = "@Id", DbType = DbType.Int32, Direction = ParameterDirection.Output };

                    inserted = await dataAccess.Sproc()
                        .AddSqlParameter(idParam)
                        .AddSqlParameter("@City", customer.City)
                        .AddSqlParameter("@Country", customer.Country)
                        .AddSqlParameter("@FirstName", customer.FirstName)
                        .AddSqlParameter("@LastName", customer.LastName)
                        .AddSqlParameter("@Phone", customer.Phone)
                        .ExecuteNonQueryAsync("dbo.SaveCustomer", conn: conn);

                    int id = idParam.GetValueOrDefault<int>();

                    if (id == default(int))
                        throw new InvalidOperationException("Id output not parsed");

                    await dataAccess.Sproc()
                        .AddSqlParameter("@CustomerId", id)
                        .ExecuteNonQueryAsync("dbo.DeleteCustomer", conn: conn);

                }

                scope.Complete();
            }

            Assert.AreEqual(1, inserted);
        }
    }
}
