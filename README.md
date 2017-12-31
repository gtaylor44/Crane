Crane is an simple to use ORM for people who like to write SQL.

Some key Features:
 * Support: SQL Server, task based async pattern, .NET >= 4.5.0 and .NET Standard >= 1.3
 * Cache queries in memory or a custom provider of your choice. 
 * Minimise the risk of common mistakes that occur when mapping manually. Be alerted when a column included in select statement does not have a corrosponding model property. 
 * Add custom mappings for column aliases so your stored procedures dont have to suffer readability issues.  
 
# Getting started
```c#
// Basic instantiation 

// Sql Server
private readonly ICraneAccess _sqlAccess = new SqlServerAccess("your connection string");

// Or use your favourite DI framework...

// .NET Core
public void ConfigureServices(IServiceCollection services)
{
    services.AddTransient<ICraneAccess>(x => new SqlServerAccess("your connection string", new MemoryCacheProvider()));
    services.AddMvc();
}

// Autofac
var builder = new ContainerBuilder();
builder.Register<ICraneAccess>(x => new SqlServerAccess("your connection string", new MemoryCacheProvider())).InstancePerDependency();

Container = builder.Build();

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
var products = _sqlAccess.Query().ExecuteReader<Product>("dbo.GetProducts");
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
 
var products = _sqlAccess.Query()
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
    var products = _sqlAccess.Query()
    .CustomColumnMapping<Product>(x => x.Id, "Product Id")
    .CustomColumnMapping<Product>(x => x.ProductName, "Product Name")
    .ExecuteReader<Product>("dbo.GetProducts");
```
-----------------------------
## Caching

Crane supports caching for tuning application performance. 

Currently only MemoryCacheProvider (which uses MemoryCache by .NET) is supported. You can easily implement your own cache provider (e.g. Redis) by extending the 
AbstractCraneCacheProvider interface. Once a cache provider is registered, you can supply 'cacheKey' as a named parameter to ExecuteReader queries. 

```c#
dataAccess
		.Query()
		.ExecuteReader<Product>("dbo.GetProducts", cacheKey: "customer_x_products");
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

// Note: Default policy if global or custom policies not added is infinite expiration. 

// Global policy applied if nothing specific is found. 
cacheProvider.SetGlobalPolicy(new SprocCachePolicy()
{
    AbsoluteExpiration = TimeSpan.FromDays(1)
});

// Regular expression will have precedence over global or default policy. Add as many policies as you need.  
cacheProvider.AddPolicy(@"user-products-.*", new SprocCachePolicy()
{
    AbsoluteExpiration = TimeSpan.FromMinutes(30)
});

_sqlAccess.RegisterCacheProvider(cacheProvider);
```

-----------------------------
## Reusing an existing DbConnection
Crane by default will manage the connection for you. If however you're in a TransactionScope and want to reuse an existing DbConnection and 
not have Crane open and close the connection for you, you can supply the named parameter 'unmanagedConn'.

```c#
dataAccess.Query().ExecuteReader<Product>("dbo.GetProducts", unmanagedConn: conn);
```

Note: 95% of the time you don't need this. Let Crane handle the connection for you for most cases. 

-----------------------------
## Table Joins

Join up to eight other related entities. When mapping a join you must supply the **partitionOn** and **callback** parameters.
Please observe the below procedure carefully and pay special attention to the columns 'ProductName' and 'Id'.
These are the two arguments for the partitionOn parameter. partitionOn arguments are separated by a pipe '|'. The callback parameter 
is a delegate and is invoked for every row that is processed. This is your chance to do any mappings for your TResult reference type. 
Because Crane reads row by row, some relationships may require an intermediate dictionary. 

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
 
product = _sqlAccess.Query()
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

_sqlAccess.Query()
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


_sqlAccess.Query()
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

Crane validates that all select columns are mapped to a corresponding model property by default. This can be disabled by setting validateSelectColumns to false. 
The below example sets an alias in stored procedure but because no custom column mapping has been setup, it's not mapped 
and throws a CraneException. The same exception message is shown if the property does not exist or a custom 
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

Customer customer = _sqlAccess.Query() 
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

