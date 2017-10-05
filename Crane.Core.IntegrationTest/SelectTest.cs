using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SprocMapperLibrary.CacheProvider;
using SprocMapperLibrary.CacheProvider.MemoryCache;
using SprocMapperLibrary.Interface;
using SprocMapperLibrary.SqlServer;
using SprocMapperLibrary.TestCommon.Model;
using SprocMapperLibrary.TestCommon;
using System;

namespace IntegrationTest
{
    [TestClass]
    public class SelectTest
    {
        // Returns all products with Id and Product Name only
        // Id has an alias of 'Product Id'
        [TestMethod]
        public void GetProductsDynamic()
        {
            ISprocMapperAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);

            var productList = dataAccess
                .Query()
                .ExecuteReader(" dbo.GetProducts")
                .ToList()
                .ConvertAll(x => new Product()
            {
                Id = x.ProductId,
                ProductName = x.ProductName
            });

            Assert.IsTrue(productList.Any());
        }

        // Returns all products with Id and Product Name only
        // Id has an alias of 'Product Id'
        [TestMethod]
        public void GetProductsDynamicWithCallback()
        {
            ISprocMapperAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);

            List<Product> productList = new List<Product>();
            dataAccess.Query()
                .ExecuteReader("dbo.GetProducts", (x) =>
                {
                    productList.Add(new Product()
                    {
                        Id = x.ProductId,
                        ProductName = x.ProductName
                    });
                });

            Assert.IsTrue(productList.Any());
        }

        [TestMethod]
        public void GetAllCustomersAndOrders()
        {
            ISprocMapperAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);

            Dictionary<int, Customer> customerDic = new Dictionary<int, Customer>();

            dataAccess.Query()
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
            var cacheProvider = new MemoryCacheProvider();

            cacheProvider.AddPolicy("GetProducts", new SprocCachePolicy()
            {
                SlidingExpiration = TimeSpan.FromSeconds(15)
            });

            ISprocMapperAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString, cacheProvider);


            var products = dataAccess.Query()
                .CustomColumnMapping<Product>(x => x.Id, "Product Id")
                .ExecuteReader<Product>("dbo.GetProducts", cacheKey: "GetProducts");

            var products2 = dataAccess.Query()
                .CustomColumnMapping<Product>(x => x.Id, "Product Id")
                .ExecuteReader<Product>("dbo.GetProducts", cacheKey: "GetProducts");

            Assert.IsTrue(products2.Count() > 0);
        }

        [TestMethod]
        public void GetProductsTextQuery()
        {
            ISprocMapperAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);

            var products = dataAccess.Query()
                .AddSqlParameter("@SupplierId", 1)
                .ExecuteReader<Product>("SELECT * FROM dbo.Product WHERE SupplierId = @SupplierId");

            Assert.IsTrue(products.Count() == 3);
        }

        [TestMethod]
        public void GetProducts_WithCallback()
        {
            ISprocMapperAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);

            List<Product> productList = new List<Product>();

            dataAccess.Query()
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
            ISprocMapperAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);

            Customer cust = null;

            dataAccess.Query()
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
            ISprocMapperAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);

            int productId = 62;
            Product product = null;


            product = dataAccess.Query()
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
            ISprocMapperAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);

            int orderId = 20;

            Order order = null;


            Dictionary<int, Order> orderDic = new Dictionary<int, Order>();

            dataAccess.Query()
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
            ISprocMapperAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);

            var suppliers = dataAccess.Query().ExecuteReader<Supplier>("dbo.GetSuppliers");
            Assert.IsTrue(suppliers.Any());
        }

        [TestMethod]
        public void GetCustomer()
        {
            ISprocMapperAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);


            var customer = dataAccess.Query()
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
            ISprocMapperAccess dataAccess = new SqlServerAccess(SqlConnectionFactory.SqlConnectionString);


            var supplier = dataAccess.Query()
                .AddSqlParameter("@SupplierName", "Bigfoot Breweries")
                .ExecuteReader<Supplier>("dbo.GetSupplierByName")
                .FirstOrDefault();

            Assert.AreEqual("Cheryl Saylor", supplier?.ContactName);

        }
    }
}
