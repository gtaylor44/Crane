<img src="http://gregnz.com/images/SprocMapper/logo.png" alt="SprocMapper logo"> 

SprocMapper is an easy to use object-relational mapper specifically designed for stored procedures. Write less lines of code and be more productive.

Key Features:
 * Support: SQL Server and MySql
 * Specificially designed for working with stored procedures.
 * Drastically Speed up the time it takes to map a stored procedure in the application layer. Minimise the risk of common mistakes that occur when mapping manually. 
 * Add custom mappings for column aliases so your stored procedures dont have to suffer readability issues. 
 * Validate that all columns in select statement are mapped to a corresponding model property (this can optionally be disabled). 
 

## Examples

Selects all products defined by 'dbo.GetProducts'.

```sql
ALTER PROCEDURE [dbo].[GetProducts]
AS
BEGIN
SELECT p.Id, p.ProductName, p.SupplierId, 
p.UnitPrice, p.Package, p.IsDiscontinued 
FROM dbo.Product p
END

```

```c#
using (SqlConnection conn = SqlConnectionFactory.GetSqlConnection())
{
    var products = conn.Sproc()
    .ExecuteReader<Product>("dbo.GetProducts");
}
```
-----------------------------
Easily add parameters. The below example gets all products with a supplier id of 2.
You can chain the AddSqlParamater method more than once if you have more than one parameter
to include. 

```sql
ALTER PROCEDURE [dbo].[GetProducts]
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
using (SqlConnection conn = SqlConnectionFactory.GetSqlConnection())
{   
    var products = conn.Sproc()
    .AddSqlParameter("@SupplierId", 2)
    .ExecuteReader<Product>("dbo.GetProducts");
}
```
-----------------------------
If you're using a column alias in your select statement, add one or
many custom column mappings depending on your procedure.

```sql
ALTER PROCEDURE [dbo].[GetProducts]
AS
BEGIN
SELECT p.Id as [Product Id], p.ProductName as [Product Name]
FROM dbo.Product p
END

```

```c#
using (SqlConnection conn = SqlConnectionFactory.GetSqlConnection())
{
    var products = conn.Sproc()
    .CustomColumnMapping<Product>(x => x.Id, "Product Id")
    .CustomColumnMapping<Product>(x => x.ProductName, "Product Name")
    .ExecuteReader<Product>("dbo.GetProducts");
}
```
-----------------------------
Join up to eight other related entities. When mapping a join you must supply the **partitionOn** and **callback** parameters.
Please observe the below procedure carefully and pay special attention to the columns 'ProductName' and 'Id'.
These are the two arguments for the partitionOn parameter. partitionOn arguments are separated by a pipe '|'. The callback parameter 
is a delegate and is invoked for every row that is processed. This is your chance to do any mappings for your TResult reference type. 
Because SprocMapper reads row by row, some relationships may require an intermediate dictionary. 

The below example retrieves a single product with an Id of 62 and its associated supplier. It's a 1:1 relationship. 

```sql
ALTER PROCEDURE [dbo].[GetProductAndSupplier]
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

using (SqlConnection conn = SqlConnectionFactory.GetSqlConnection())
{    
    product = conn.Sproc()
    .AddSqlParameter("@Id", 62)
    .ExecuteReader<Product, Supplier>("[dbo].[GetProductAndSupplier]", (p, s) =>
    {
        p.Supplier = s;
    }, partitionOn: "ProductName|Id")
    .FirstOrDefault();
}

Assert.AreEqual("Tarte au sucre", product?.ProductName);
Assert.AreEqual("Chantal Goulet", product?.Supplier.ContactName);

```
-----------------------------
1:M example between customer (if exists) and their order(s).

```sql
ALTER PROCEDURE [dbo].[GetCustomerAndOrders]
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

using (SqlConnection conn = SqlConnectionFactory.GetSqlConnection())
{
    conn.Sproc()
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
}

Assert.IsNotNull(cust);
Assert.AreEqual(13, cust.CustomerOrders.Count);
```
-----------------------------
SprocMapper validates that all select columns are mapped to a corresponding model property by default. This can be disabled by setting validateSelectColumns to false. 
The below example sets an alias in stored procedure but because no custom column mapping has been setup, it's not mapped 
and throws a SprocMapperException. The same exception message is shown if the property does not exist or a custom 
column mapping is incorrect. Note that when mapping joins, it's important to have accurate 'partitionOn' arguments to avoid mixed results. 

```sql
ALTER PROCEDURE [dbo].[GetCustomer]
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
using (SqlConnection conn = SqlConnectionFactory.GetSqlConnection())
{
    Customer customer = conn.Sproc() 
    .AddSqlParameter("@CustomerId", 6)
    .ExecuteReader<Customer>("dbo.GetCustomer", validateSelectColumns: true)
    .FirstOrDefault();
}
```
#### Exception message:
```
'validateSelectColumns' flag is set to TRUE

The following columns from the select statement in 'dbo.GetCustomer' have not been mapped to target model 'Customer'.

Select column: 'First Name'
Target model: 'Customer'

```
-----------------------------
Execute a stored procedure without a result set.

```sql
ALTER PROCEDURE [dbo].[SaveCustomer]
	@Id int output,
	@City nvarchar(40),
	@Country nvarchar(40),
	@Phone nvarchar(20),
	@FirstName nvarchar(40),
	@LastName nvarchar(40)
AS
BEGIN
    SET NOCOUNT ON;
    
	INSERT INTO dbo.Customer
	(
        FirstName,
        LastName,
        City,
        Country,
        Phone
	)

	VALUES
	(
        @FirstName,
        @LastName,
        @City,
        @Country,
        @Phone
	)

	SET @Id = SCOPE_IDENTITY();
    
END
```
```c#

Customer customer = new Customer()
{
    City = "Auckland",
    Country = "New Zealand",
    FirstName = "Greg",
    LastName = "Taylor",
    Phone = "111"
};

int insertedRecords = 0;
            
using (SqlConnection conn = SqlConnectionFactory.GetSqlConnection())
{
    SqlParameter idParam = new SqlParameter() 
    { 
        ParameterName = "@Id", 
        DbType = DbType.Int32, 
        Direction = ParameterDirection.Output 
    };

    insertedRecords = conn.Sproc()
        .AddSqlParameter(idParam)
        .AddSqlParameter("@City", customer.City)
        .AddSqlParameter("@Country", customer.Country)
        .AddSqlParameter("@FirstName", customer.FirstName)
        .AddSqlParameter("@LastName", customer.LastName)
        .AddSqlParameter("@Phone", customer.Phone)
        .ExecuteNonQuery("dbo.SaveCustomer");

    int outputId = idParam.GetValueOrDefault<int>();
}

Assert.IsTrue(insertedRecords == 1);
```
-----------------------------

### Performance
Internally, SprocMapper uses FastMember for caching property members and mutating values. Performance is more dependant on how well your SQL is written, indexes, hardware, etc. 
Please feel free to blog, compare and review.

