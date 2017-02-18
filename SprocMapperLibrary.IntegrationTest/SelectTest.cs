using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model;
using SprocMapperLibrary;
using SprocMapperLibrary.Core;
using SprocMapperLibrary.TestCommon;

namespace IntegrationTest
{
    [TestClass]
    public class SelectTest
    {

        // Returns all products with Id and Product Name only
        // Id has an alias of 'Product Id'
        [TestMethod]
        public void GetProducts()
        {
            using (
                SqlConnection conn = GetSqlConnection())
            {
                var products = conn.Select()
                    .CustomColumnMapping<Product>(x => x.Id, "Product Id")
                    .ExecuteReader<Product, Supplier>(conn, "dbo.GetProducts", (p, s) => { }, "Product Id|Id");

                Assert.IsNotNull(products);
            }
        }


        // 1:M relationship example
        [TestMethod]
        public void SelectSingleCustomerAndOrders()
        {
            Customer cust = null;

            using (SqlConnection conn = GetSqlConnection())
            {

                conn.Select()
                    .AddSqlParameter("@FirstName", "Thomas")
                    .AddSqlParameter("@LastName", "Hardy")
                    .CustomColumnMapping<Order>(x => x.Id, "OrderId")
                    .ExecuteReader<Customer, Order>(conn, "dbo.GetCustomerAndOrders", (c, o) =>
                    {
                        if (cust == null)
                        {
                            cust = c;
                            cust.CustomerOrders = new List<Order>();
                        }

                        if (o.Id != default(int))
                            cust.CustomerOrders.Add(o);

                    }, "Id|OrderId");
            }

            Assert.AreEqual(13, cust.CustomerOrders.Count);
            Assert.IsNotNull(cust);
        }

        // 1:1 relationship example
        [TestMethod]
        public void GetProductAndSupplier()
        {
            int productId = 62;
            Product product = null;

            using (SqlConnection conn = GetSqlConnection())
            {              
                product = conn.Select()
                    .AddSqlParameter("@Id", productId)
                    .ExecuteReader<Product, Supplier>(conn, "[dbo].[GetProductAndSupplier]", (p, s) =>
                    {
                        p.Supplier = s;

                    }, "ProductName|Id").FirstOrDefault();
            }

            Assert.AreEqual("Tarte au sucre", product?.ProductName);
            Assert.AreEqual("Chantal Goulet", product?.Supplier.ContactName);

            Assert.AreNotEqual(0, product?.Supplier.Id);
        }

        [TestMethod]
        public void GetOrderAndProducts()
        {
            int orderId = 20;

            Order order = null;

            using (SqlConnection conn = GetSqlConnection())
            {
                Dictionary<int, Order> orderDic = new Dictionary<int, Order>();

                conn.Select()
                .AddSqlParameter("@OrderId", orderId)
                .CustomColumnMapping<Product>(x => x.UnitPrice, "Price")   
                .ExecuteReader<Order, OrderItem, Product>(conn, "dbo.GetOrder", (o, oi, p) =>
                    {
                        Order ord;
                        if (!orderDic.TryGetValue(o.Id, out ord))
                        {
                            orderDic.Add(o.Id, o);
                            o.OrderItemList = new List<OrderItem>();
                        }

                        order = orderDic[o.Id];
                        oi.Product = p;
                        order.OrderItemList.Add(oi);
                    }, "Id|unitprice|productname");
            }

            Assert.IsNotNull(order);
        }

        [TestMethod]
        public void GetSuppliers()
        {
            using (SqlConnection conn = GetSqlConnection())
            {
                var suppliers = conn.Select().ExecuteReader<Supplier>(conn, "dbo.GetSuppliers");

                Assert.IsTrue(suppliers.Any());
            }
        }

        [TestMethod]
        public void GetCustomer()
        {
            using (SqlConnection conn = GetSqlConnection())
            {
                var customer = conn.Select()
                    .AddSqlParameter("@CustomerId", 6)
                    .ExecuteReader<Customer>(conn, "dbo.GetCustomer")
                    .FirstOrDefault();

                Assert.AreEqual("Hanna", customer?.FirstName);
                Assert.AreEqual("Moos", customer?.LastName);
                Assert.AreEqual("Mannheim", customer?.City);
                Assert.AreEqual("Germany", customer?.Country);
            }
        }

        [TestMethod]
        public void GetSupplierByName()
        {
            using (SqlConnection conn = GetSqlConnection())
            {
                var supplier = conn.Select()
                    .AddSqlParameter("@SupplierName", "Bigfoot Breweries")
                    .ExecuteReader<Supplier>(conn, "dbo.GetSupplierByName")
                    .FirstOrDefault();

                Assert.AreEqual("Cheryl Saylor", supplier?.ContactName);
            }
        }

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
                using (SqlConnection conn = GetSqlConnection())
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

        [TestMethod]
        [MyExpectedException(typeof(SprocMapperException), "Custom column mapping must map to a unique property. A property with the name 'ProductName' already exists.")]
        public void CustomColumnName_MustBeUniqueToClass()
        {
            using (SqlConnection conn = GetSqlConnection())
            {
                conn.Select()
                    .CustomColumnMapping<Product>(x => x.Package, "ProductName")
                    .ExecuteReader<Product>(conn, "dbo.GetProducts");
            }
        }

        private SqlConnection GetSqlConnection()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings["SprocMapperTest"].ConnectionString);
        }
    }
}
