using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Crane;
using Crane.Interface;
using Crane.SqlServer;
using Crane.TestCommon;
using Crane.TestCommon.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTest
{
    [TestClass]
    public class SelectAsyncTest
    {
        // Returns all products with Id and Product Name only
        // Id has an alias of 'Product Id'
        [TestMethod]
        public async Task GetProducts()
        {
            ICraneAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);

            var products = await dataAccess.Query()
                .CustomColumnMapping<Product>(x => x.Id, "Product Id")
                .ExecuteReaderAsync<Product>("dbo.GetProducts");

            Assert.IsNotNull(products);

        }


        // 1:M relationship example
        [TestMethod]
        public async Task SelectSingleCustomerAndOrders()
        {
            ICraneAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);
            Customer cust = null;


            await dataAccess.Query()
                .AddSqlParameter("@FirstName", "Thomas")
                .AddSqlParameter("@LastName", "Hardy")
                .CustomColumnMapping<Order>(x => x.Id, "OrderId")
                .ExecuteReaderAsync<Customer, Order>("dbo.GetCustomerAndOrders", (c, o) =>
                {
                    if (cust == null)
                    {
                        cust = c;
                        cust.CustomerOrders = new List<Order>();
                    }

                    if (o.Id != default(int))
                        cust.CustomerOrders.Add(o);

                }, "Id|OrderId");


            Assert.AreEqual(13, cust.CustomerOrders.Count);
            Assert.IsNotNull(cust);
        }

        // 1:1 relationship example
        [TestMethod]
        public async Task GetProductAndSupplier()
        {
            ICraneAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);
            int productId = 62;
            Product product = null;


            product = (await dataAccess.Query()
                .AddSqlParameter("@Id", productId)
                .ExecuteReaderAsync<Product, Supplier>("[dbo].[GetProductAndSupplier]", (p, s) =>
                {
                    p.Supplier = s;

                }, "ProductName|Id")).FirstOrDefault();


            Assert.AreEqual("Tarte au sucre", product?.ProductName);
            Assert.AreEqual("Chantal Goulet", product?.Supplier.ContactName);

            Assert.AreNotEqual(0, product?.Supplier.Id);
        }

        [TestMethod]
        public async Task GetOrderAndProducts()
        {
            ICraneAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);
            int orderId = 20;

            Order order = null;


            Dictionary<int, Order> orderDic = new Dictionary<int, Order>();

            await dataAccess.Query()
            .AddSqlParameter("@OrderId", orderId)
            .CustomColumnMapping<Product>(x => x.UnitPrice, "Price")
            .ExecuteReaderAsync<Order, OrderItem, Product>("dbo.GetOrder", (o, oi, p) =>
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


            Assert.IsNotNull(order);
        }

        [TestMethod]
        public async Task GetSuppliers()
        {
            ICraneAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);

            var suppliers = await dataAccess
                .Query()
                .ExecuteReaderAsync<Supplier>("dbo.GetSuppliers");

            Assert.IsTrue(suppliers.Any());

        }

        [TestMethod]
        public async Task GetCustomer()
        {
            ICraneAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);

            var customer = (await dataAccess.Query()
                .AddSqlParameter("@CustomerId", 6)
                .ExecuteReaderAsync<Customer>("dbo.GetCustomer"))
                .FirstOrDefault();

            Assert.AreEqual("Hanna", customer?.FirstName);
            Assert.AreEqual("Moos", customer?.LastName);
            Assert.AreEqual("Mannheim", customer?.City);
            Assert.AreEqual("Germany", customer?.Country);

        }

        [TestMethod]
        public async Task GetSupplierByName()
        {
            ICraneAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);

            var supplier = (await dataAccess.Query()
                .AddSqlParameter("@SupplierName", "Bigfoot Breweries")
                .ExecuteReaderAsync<Supplier>("dbo.GetSupplierByName"))
                .FirstOrDefault();

            Assert.AreEqual("Cheryl Saylor", supplier?.ContactName);

        }

        [TestMethod]
        public async Task InsertCustomerThenDelete()
        {
            ICraneAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);

            Customer customer = new Customer()
            {
                City = "Auckland",
                Country = "New Zealand",
                FirstName = "Greg",
                LastName = "Taylor",
                Phone = "021222222"
            };

            int inserted = 0;



            using (SqlConnection conn = new SqlConnection(SqlConnectionFactory.SqlConnectionString))
            {
                conn.Open();
                SqlParameter idParam = new SqlParameter() { ParameterName = "@Id", DbType = DbType.Int32, Direction = ParameterDirection.Output };

                inserted = await dataAccess.Command()
                    .AddSqlParameter(idParam)
                    .AddSqlParameter("@City", customer.City)
                    .AddSqlParameter("@Country", customer.Country)
                    .AddSqlParameter("@FirstName", customer.FirstName)
                    .AddSqlParameter("@LastName", customer.LastName)
                    .AddSqlParameter("@Phone", customer.Phone)
                    .ExecuteNonQueryAsync("dbo.SaveCustomer", dbConnection: conn);

                int id = idParam.GetValueOrDefault<int>();

                if (id == default(int))
                    throw new InvalidOperationException("Id output not parsed");

                await dataAccess.Command()
                    .AddSqlParameter("@CustomerId", id)
                    .ExecuteNonQueryAsync("dbo.DeleteCustomer", dbConnection: conn);
            }

            Assert.AreEqual(1, inserted);
        }
    }
}
