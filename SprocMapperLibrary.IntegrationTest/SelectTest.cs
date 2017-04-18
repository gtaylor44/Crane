using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SprocMapperLibrary;
using SprocMapperLibrary.SqlServer;
using SprocMapperLibrary.TestCommon;
using SprocMapperLibrary.TestCommon.Model;
using SqlBulkTools;

namespace IntegrationTest
{
    [TestClass]
    public class SelectTest
    {
        [TestMethod]
        public void GetAllCustomersAndOrders()
        {
            SqlServerAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);

            Dictionary<int, Customer> customerDic = new Dictionary<int, Customer>();

            dataAccess.Sproc()
                .ExecuteReader<Customer, Order>("dbo.GetAllCustomersAndOrders", (c, o) =>
                {
                    Customer customer;

                    if (!customerDic.TryGetValue(c.Id, out customer))
                    {
                        customer = c;
                        customer.CustomerOrders = new List<Order>();
                        customerDic.Add(customer.Id, customer);
                    }

                    if (o != null)
                        customer.CustomerOrders.Add(o);

                }, partitionOn: "Id|Id");

            Assert.IsTrue(customerDic.Count > 0);
        }

        // Returns all products with Id and Product Name only
        // Id has an alias of 'Product Id'
        [TestMethod]
        public void GetProducts()
        {
            SqlServerAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);

            var products = dataAccess.Sproc()
                .CustomColumnMapping<Product>(x => x.Id, "Product Id")
                .ExecuteReader<Product>("dbo.GetProducts");

            Assert.IsNotNull(products);
        }

        [TestMethod]
        public void GetProducts_WithCallback()
        {
            SqlServerAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);

            List<Product> productList = new List<Product>();

            dataAccess.Sproc()
                .CustomColumnMapping<Product>(x => x.Id, "Product Id")
                .ExecuteReader<Product>("dbo.GetProducts", (product) =>
                {
                    // do something special with product
                    productList.Add(product);
                });

            Assert.IsTrue(productList.Count > 0);

        }

        // 1:M relationship example
        [TestMethod]
        public void SelectSingleCustomerAndOrders()
        {
            SqlServerAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);

            Customer cust = null;

            dataAccess.Sproc()
                .AddSqlParameter("@FirstName", "Thomas")
                .AddSqlParameter("@LastName", "Hardy")
                .CustomColumnMapping<Order>(x => x.Id, "OrderId")
                .ExecuteReader<Customer, Order>("dbo.GetCustomerAndOrders", (c, o) =>
                {
                    if (cust == null)
                    {
                        cust = c;
                        cust.CustomerOrders = new List<Order>();
                    }

                    if (o.Id != default(int))
                        cust.CustomerOrders.Add(o);

                }, partitionOn: "Id|OrderId");


            Assert.AreEqual(13, cust.CustomerOrders.Count);
            Assert.IsNotNull(cust);
        }

        // 1:1 relationship example
        [TestMethod]
        public void GetProductAndSupplier()
        {
            SqlServerAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);

            int productId = 62;
            Product product = null;


            product = dataAccess.Sproc()
                .AddSqlParameter("@Id", productId)
                .ExecuteReader<Product, Supplier>("[dbo].[GetProductAndSupplier]", (p, s) =>
                {
                    p.Supplier = s;

                }, partitionOn: "ProductName|Id").FirstOrDefault();


            Assert.AreEqual("Tarte au sucre", product?.ProductName);
            Assert.AreEqual("Chantal Goulet", product?.Supplier.ContactName);

            Assert.AreNotEqual(0, product?.Supplier.Id);
        }

        [TestMethod]
        public void GetOrderAndProducts()
        {
            SqlServerAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);

            int orderId = 20;

            Order order = null;


            Dictionary<int, Order> orderDic = new Dictionary<int, Order>();

            dataAccess.Sproc()
            .AddSqlParameter("@OrderId", orderId)
            .CustomColumnMapping<Product>(x => x.UnitPrice, "Price")
            .ExecuteReader<Order, OrderItem, Product>("dbo.GetOrder", (o, oi, p) =>
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
                }, partitionOn: "Id|unitprice|productname");


            Assert.IsNotNull(order);
        }

        [TestMethod]
        public void GetSuppliers()
        {
            SqlServerAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);

            var suppliers = dataAccess.Sproc().ExecuteReader<Supplier>("dbo.GetSuppliers");
            Assert.IsTrue(suppliers.Any());
        }

        [TestMethod]
        public void GetCustomer()
        {
            SqlServerAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);


            var customer = dataAccess.Sproc()
                .AddSqlParameter("@CustomerId", 6)
                .ExecuteReader<Customer>("dbo.GetCustomer", validateSelectColumns: true)
                .FirstOrDefault();

            Assert.AreEqual("Hanna", customer?.FirstName);
            Assert.AreEqual("Moos", customer?.LastName);
            Assert.AreEqual("Mannheim", customer?.City);
            Assert.AreEqual("Germany", customer?.Country);

        }

        [TestMethod]
        public void GetSupplierByName()
        {
            SqlServerAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);


            var supplier = dataAccess.Sproc()
                .AddSqlParameter("@SupplierName", "Bigfoot Breweries")
                .ExecuteReader<Supplier>("dbo.GetSupplierByName")
                .FirstOrDefault();

            Assert.AreEqual("Cheryl Saylor", supplier?.ContactName);

        }



        [TestMethod]
        [MyExpectedException(typeof(SprocMapperException), "Custom column mapping must map to a unique property. A property with the name 'ProductName' already exists.")]
        public void CustomColumnName_MustBeUniqueToClass()
        {
            SqlServerAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);

            dataAccess.Sproc()
                .CustomColumnMapping<Product>(x => x.Package, "ProductName")
                .ExecuteReader<Product>("dbo.GetProducts");
        }

        [TestMethod]
        public void SaveGetDataTypes()
        {
            SqlServerAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);

            BulkOperations bulk = new BulkOperations();

            TestDataType dataTypeTest = new TestDataType()
            {
                IntTest = 1,
                //SmallIntTest = 3433,
                BigIntTest = 342324324324324324,
                TinyIntTest = 126,
                DateTimeTest = DateTime.UtcNow,
                DateTime2Test = new DateTime(2008, 12, 12, 10, 20, 30),
                DateTest = new DateTime(2007, 7, 5, 20, 30, 10),
                TimeTest = new TimeSpan(23, 32, 23),
                SmallDateTimeTest = new DateTime(2005, 7, 14),
                BinaryTest = new byte[] { 0, 3, 3, 2, 4, 3 },
                VarBinaryTest = new byte[] { 3, 23, 33, 243 },
                DecimalTest = 178.43M,
                MoneyTest = 24333.99M,
                SmallMoneyTest = 103.32M,
                RealTest = 32.53F,
                NumericTest = 154343.3434342M,
                FloatTest = 232.43F,
                FloatTest2 = 43243.34,
                TextTest = "This is some text.",
                GuidTest = Guid.NewGuid(),
                CharTest = "Some",
                XmlTest = "<title>The best SQL Bulk tool</title>",
                NCharTest = "SomeText",
                ImageTest = new byte[] { 3, 3, 32, 4 }
            };

            using (SqlConnection conn = SqlConnectionFactory.GetSqlConnection())
            {
                bulk.Setup<TestDataType>()
                    .ForObject(dataTypeTest)
                    .WithTable("TestDataTypes")
                    .AddAllColumns()
                    .Upsert()
                    .MatchTargetOn(x => x.IntTest)
                    .Commit(conn);

                var result = dataAccess.Sproc()
                    .ExecuteReader<TestDataType>("dbo.GetTestDataTypes", conn: conn)
                    .SingleOrDefault();

                Assert.AreEqual(1, result?.IntTest);
            }
        }
    }
}
