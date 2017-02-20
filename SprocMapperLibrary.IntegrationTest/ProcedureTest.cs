﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SprocMapperLibrary;
using SprocMapperLibrary.TestCommon;
using SprocMapperLibrary.TestCommon.Model;

namespace IntegrationTest
{
    [TestClass]
    public class ProcedureTest
    {
        [TestMethod]
        public void InsertCustomerThenDelete()
        {
            Customer customer = new Customer()
            {
                City = "Auckland",
                Country = "New Zealand",
                FirstName = "Greg",
                LastName = "Taylor",
                Phone = "021222222"
            };

            int inserted = 0;


            using (TransactionScope scope = new TransactionScope())
            {
                using (SqlConnection conn = SqlConnectionFactory.GetSqlConnection())
                {
                    conn.Open();
                    SqlParameter idParam = new SqlParameter() { ParameterName = "@Id", DbType = DbType.Int32, Direction = ParameterDirection.Output };

                    inserted = conn.Procedure()
                        .AddSqlParameter(idParam)
                        .AddSqlParameter("@City", customer.City)
                        .AddSqlParameter("@Country", customer.Country)
                        .AddSqlParameter("@FirstName", customer.FirstName)
                        .AddSqlParameter("@LastName", customer.LastName)
                        .AddSqlParameter("@Phone", customer.Phone)
                        .ExecuteNonQuery(conn, "dbo.SaveCustomer");

                    int id = idParam.GetValueOrDefault<int>();

                    if (id == default(int))
                        throw new InvalidOperationException("Id output not parsed");

                    conn.Procedure()
                        .AddSqlParameter("@CustomerId", id)
                        .ExecuteNonQuery(conn, "dbo.DeleteCustomer");

                }

                scope.Complete();
            }

            Assert.AreEqual(1, inserted);
        }
    }
}