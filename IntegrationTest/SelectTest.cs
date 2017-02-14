using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
        public void SelectSingleTable()
        {
            using (SqlConnection conn =
                new SqlConnection(ConfigurationManager.ConnectionStrings["SprocMapperTest"].ConnectionString))
            {
                var result = conn.Select()
                    .ExecuteReader<President>(conn, "dbo.GetPresidentList2");

                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        public void SelectWithJoin()
        {
            using (SqlConnection conn =
                new SqlConnection(ConfigurationManager.ConnectionStrings["SprocMapperTest"].ConnectionString))
            {
                Dictionary<int, President> dic = new Dictionary<int, President>();

                conn.Select()
                    .AddMapping(PropertyMapper
                            .MapObject<PresidentAssistant>()
                            .CustomColumnMapping(x => x.Id, "Assistant Id")
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
                    });

                Assert.IsNotNull(dic.Values);
            }
        }

        [TestMethod]
        public void GetProducts()
        {
            using (
                SqlConnection conn =
                    new SqlConnection(ConfigurationManager.ConnectionStrings["SprocMapperTest"].ConnectionString))
            {
                var products = conn.Select()
                    .AddMapping(PropertyMapper.MapObject<Product>().CustomColumnMapping(x => x.Id, "Product Id"))
                    .ExecuteReader<Product>(conn, "dbo.GetProducts", strictValidation: true);

                Assert.IsNotNull(products);
            }
        }

        [TestMethod]
        public void SelectSingleCustomerAndOrders()
        {
            string customerFirstName = "Thomas";
            string customerLastName = "Hardy";

            Customer cust = null;

            using (SqlConnection conn = SqlConnectionFactory())
            {
               
                conn.Select()
                    .AddSqlParameter("@FirstName", SqlDbType.NVarChar, customerFirstName)
                    .AddSqlParameter("@LastName", SqlDbType.NVarChar, customerLastName)
                    .AddMapping(PropertyMapper.MapObject<Order>().CustomColumnMapping(x => x.Id, "OrderId"))
                    .ExecuteReader<Customer, Order>(conn, "dbo.GetCustomerAndOrders", (c, o) =>
                    {
                        if (cust == null)
                        {
                            cust = c;
                            if (cust.CustomerOrders == null)
                                cust.CustomerOrders = new List<Order>();
                        }
                        
                        cust.CustomerOrders.Add(o);
                    }, strictValidation:true);
            }

            Assert.IsNotNull(cust);
        }

        [TestMethod]
        public void GetProductAndSupplier()
        {
            int productId = 62;
            Product product = null;

            using (SqlConnection conn = SqlConnectionFactory())
            {
                product = conn.Select()
                    .AddMapping(PropertyMapper.MapObject<Product>().CustomColumnMapping(x => x.Id, "ProductId"))
                    .AddMapping(PropertyMapper.MapObject<Supplier>().CustomColumnMapping(x => x.Id, "SupplierId"))
                    .AddSqlParameter("@Id", SqlDbType.Int, productId)
                    .ExecuteReader<Product, Supplier>(conn, "[dbo].[GetProductAndSupplier]", (p, s) =>
                    {
                        p.Supplier = s;
                    }).First();
            }

            Assert.AreEqual("Tarte au sucre", product.ProductName);
            Assert.AreEqual("Chantal Goulet", product.Supplier.ContactName);

            Assert.AreNotEqual(0, product.Supplier.Id);
        }

        private SqlConnection SqlConnectionFactory()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings["SprocMapperTest"].ConnectionString);
        }
    }
}
