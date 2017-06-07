<img src="http://gregnz.com/images/SprocMapper/logo.png" alt="SprocMapper logo"> 

SprocMapper is an easy to use object-relational mapper specifically designed for stored procedures. Write less lines of code and be more productive.

Key Features:
 * Support: SQL Server and MySql
 * Specificially designed for working with stored procedures.
 * Caching
 * Drastically Speed up the time it takes to map a stored procedure in the application layer. Minimise the risk of common mistakes that occur when mapping manually. 
 * Add custom mappings for column aliases so your stored procedures dont have to suffer readability issues. 
 * Validate that all columns in select statement are mapped to a corresponding model property (this can optionally be disabled). 
 
# Getting started
```c#
// Sql Server
private readonly ISprocMapperAccess _sqlAccess = new SqlServerAccess("your connection string");

// MYSQL Server
private readonly ISprocMapperAccess _mySqlAccess = new MySqlServerAccess("your connection string");
```

## Examples

Selects all products defined by 'dbo.GetProducts'.

```sql
CREATE PROCEDURE [dbo].[GetProducts]
AS
BEGIN
    SELECT p.Id, p.ProductName, p.SupplierId, 
    p.UnitPrice, p.Package, p.IsDiscontinued 
    FROM dbo.Product p
END

```

```c#
// Returns IEnumerable of type Product.
var products = _sqlAccess.Sproc().ExecuteReader<Product>("dbo.GetProducts");
```
-----------------------------
Easily add parameters. The below example gets all products with a supplier id of 2.
You can chain the AddSqlParamater method more than once if you have more than one parameter
to include. 

```sql
CREATE PROCEDURE [dbo].[GetProducts]
	@SupplierId int
AS
BEGIN
    SELECT p.Id, p.ProductName, p.UnitPrice,
    p.Package, p.IsDiscontinued 
    FROM dbo.Product p
    WHERE p.SupplierId = @SupplierId
END
```

```c#
 
var products = _sqlAccess.Sproc()
    .AddSqlParameter("@SupplierId", 2)
    .ExecuteReader<Product>("dbo.GetProducts");

```
-----------------------------
If you're using a column alias in your select statement, add one or
many custom column mappings depending on your procedure.

```sql
CREATE PROCEDURE [dbo].[GetProducts]
AS
BEGIN
    SELECT p.Id as [Product Id], p.ProductName as [Product Name]
    FROM dbo.Product p
END

```

```c#
    var products = _sqlAccess.Sproc()
    .CustomColumnMapping<Product>(x => x.Id, "Product Id")
    .CustomColumnMapping<Product>(x => x.ProductName, "Product Name")
    .ExecuteReader<Product>("dbo.GetProducts");
```
-----------------------------
## Caching

SprocMapper supports caching for tuning application performance. Here's how to register a cache provider:

```c#
_sqlAccess.RegisterCacheProvider(new MemoryCacheProvider());
```

Currently only MemoryCacheProvider (which uses MemoryCache by .NET) is provided. You can easily implement your own cache provider (e.g. Redis) by implementing 
AbstractCacheProvider. If you do implement your own cache provider, I would appreciate if you could also create a pull request 
so others can benefit from your work. 

Once a cache provider is registered, you can supply 'cacheKey' as a named parameter to ExecuteReader commands. 

```c#
dataAccess.Sproc().ExecuteReader<Product>("dbo.GetProducts", cacheKey: "customer_x_products");
```

If you want to force refresh the cache so a fresh copy is retrieved next call, you can use the RemoveFromCache method.

```c#
dataAccess.RemoveFromCache("customer_x_products");
```
-----------------------------
## Advanced Caching

Apply policies to cache keys globally or with a regular expresssion. 

```c#
var cacheProvider = new MemoryCacheProvider();

// Global policy applied if nothing specific is found. 
cacheProvider.SetGlobalPolicy(new SprocCachePolicy()
{
    InfiniteExpiration = true
});

// This will have precedence over global or default policy 
cacheProvider.AddPolicy("GetProducts", new SprocCachePolicy()
{
    CacheKeyRegExp = @"user-products-.*",
    AbsoluteExpiration = TimeSpan.FromMinutes(30)
});

_sqlAccess.RegisterCacheProvider(cacheProvider);
```

-----------------------------
## Reusing an existing DbConnection
SprocMapper by default will manage the connection for you. If however you're in a TransactionScope and want to reuse an existing DbConnection and 
not have SprocMapper open and close the connection for you, you can supply the named parameter 'unmanagedConn'.

```c#
dataAccess.Sproc().ExecuteReader<Product>("dbo.GetProducts", unmanagedConn: conn);
```

Note: 95% of the time you don't need this. Let SprocMapper handle the connection for you for most cases. 

-----------------------------
## Table Joins

Join up to eight other related entities. When mapping a join you must supply the **partitionOn** and **callback** parameters.
Please observe the below procedure carefully and pay special attention to the columns 'ProductName' and 'Id'.
These are the two arguments for the partitionOn parameter. partitionOn arguments are separated by a pipe '|'. The callback parameter 
is a delegate and is invoked for every row that is processed. This is your chance to do any mappings for your TResult reference type. 
Because SprocMapper reads row by row, some relationships may require an intermediate dictionary. 

The below example retrieves a single product with an Id of 62 and its associated supplier. It's a 1:1 relationship. 

```sql
CREATE PROCEDURE [dbo].[GetProductAndSupplier]
	@Id int
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT p.ProductName, p.UnitPrice, p.Package, p.IsDiscontinued,
    s.Id, s.CompanyName, s.ContactName, s.ContactTitle, s.City, 
    s.Country, s.Phone, s.Fax 
    FROM dbo.Product p
    INNER JOIN dbo.Supplier s
    ON p.SupplierId = s.Id
    WHERE p.Id = @Id
END
```

```c#
Product product = null;
 
product = _sqlAccess.Sproc()
    .AddSqlParameter("@Id", 62)
    .ExecuteReader<Product, Supplier>("[dbo].[GetProductAndSupplier]", (p, s) =>
    {
        p.Supplier = s;
    }, partitionOn: "ProductName|Id")
    .FirstOrDefault();


Assert.AreEqual("Tarte au sucre", product?.ProductName);
Assert.AreEqual("Chantal Goulet", product?.Supplier.ContactName);

```
-----------------------------
Get customer and their order(s).

```sql
CREATE PROCEDURE [dbo].[GetCustomerAndOrders]
	@FirstName nvarchar(40),
	@LastName nvarchar(40)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT c.Id, c.FirstName, c.LastName, c.City, c.Country, c.Phone, 
    o.Id as [OrderId], o.OrderDate, o.OrderNumber, o.TotalAmount
    FROM dbo.Customer c
    LEFT JOIN dbo.[Order] o
    ON o.CustomerId = c.Id
    WHERE c.FirstName = @FirstName
    AND c.LastName = @LastName
END
```

```c#
Customer cust = null;

_sqlAccess.Sproc()
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
        
        if (o != null)
            cust.CustomerOrders.Add(o);

    }, partitionOn: "Id|OrderId");


Assert.IsNotNull(cust);
Assert.AreEqual(13, cust.CustomerOrders.Count);
```
-----------------------------
A slightly more complex example getting all customers and all orders. This demonstrates the use of a dictionary to only get distinct customers. 
```sql
CREATE PROCEDURE GetAllCustomersAndOrders
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM dbo.Customer c
    LEFT JOIN dbo.[Order] o
    ON c.Id = o.CustomerId
END
GO
```
```c#
Dictionary<int, Customer> customerDic = new Dictionary<int, Customer>();


_sqlAccess.Sproc()
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
    }

    Assert.IsTrue(customerDic.Count > 0);

```
-----------------------------
## Catch bugs early

SprocMapper validates that all select columns are mapped to a corresponding model property by default. This can be disabled by setting validateSelectColumns to false. 
The below example sets an alias in stored procedure but because no custom column mapping has been setup, it's not mapped 
and throws a SprocMapperException. The same exception message is shown if the property does not exist or a custom 
column mapping is incorrect. Note that when mapping joins, it's important to have accurate 'partitionOn' arguments to avoid mixed results. 

```sql
CREATE PROCEDURE [dbo].[GetCustomer]
	@CustomerId int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT c.FirstName as [First Name], c.LastName, c.City, c.Country, c.Phone 
    FROM Customer c 
    WHERE Id = @CustomerId
END
```
```c#

Customer customer = _sqlAccess.Sproc() 
    .AddSqlParameter("@CustomerId", 6)
    .ExecuteReader<Customer>("dbo.GetCustomer", validateSelectColumns: true)
    .FirstOrDefault();

```
#### Exception message:
```
'validateSelectColumns' flag is set to TRUE

The following columns from the select statement in 'dbo.GetCustomer' have not been mapped to target model 'Customer'.

Select column: 'First Name'
Target model: 'Customer'

```
-----------------------------
### Performance
Internally, SprocMapper has a dependency on FastMember for efficient dynamic reading/writing of property member values. Performance is more dependant on how well your SQL is written, indexes, hardware, etc. 
Please feel free to blog, compare and review.

