#SprocMapper
-----------------------------
SprocMapper is a productivity tool for mapping SQL result sets from stored procedures into strongly typed objects. 

* Advantages of using SprocMapper:
 * Speeds up the time it takes to map a stored procedure in the application layer.
 * Minimises the risk of making common mistakes by manually mapping each column (yes, you are human).

##Examples

Select all products defined by a stored procedure named 'dbo.GetProducts'.

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
    var products = conn.Select().ExecuteReader<Product>(conn, "dbo.GetProducts");
}
```

Easily add parameters.

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
    /* Get all products with a supplier id of 2. AddSqlParamater can 
    be chained more than once if you have more than one parameter to 
    include. */
    
    var products = conn.Select()
    .AddSqlParameter("@SupplierId", 2)
    .ExecuteReader<Product>(conn, "dbo.GetProducts");
}
```

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
    var products = conn.Select()
    .CustomColumnMapping<Product>(x => x.Id, "Product Id")
    .CustomColumnMapping<Product>(x => x.ProductName, "Product Name")
    .ExecuteReader<Product>(conn, "dbo.GetProducts");
}
```

Join up to seven other related entities. When mapping a join you must supply the partitionOn and callback parameters.
Please observe the below procedure carefully and pay special attention to the columns 'ProductName' and 'Id'.
These are the two arguments for the partitionOn parameter. The callback parameter is a delegate and is called
for every row that is processed. This is your chance to do any mappings for your TResult reference type. Because SprocMapper
reads row by row, some relationships may require an intermediate dictionary. 

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
    // Retrieves a single product with an Id of 62 and it's associated supplier. 
    
    product = conn.Select()
    .AddSqlParameter("@Id", 62)
    .ExecuteReader<Product, Supplier>(conn, "[dbo].[GetProductAndSupplier]", (p, s) =>
    {
        p.Supplier = s;
    }, partitionOn: "ProductName|Id")
    .FirstOrDefault();
}

Assert.AreEqual("Tarte au sucre", product?.ProductName);
Assert.AreEqual("Chantal Goulet", product?.Supplier.ContactName);

```

One to many example.

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
        
        // SprocMapper handles DbNull.Value gracefully.
        if (o.Id != default(int))
            cust.CustomerOrders.Add(o);

    }, partitionOn: "Id|OrderId");
}

Assert.IsNotNull(cust);
Assert.AreEqual(13, cust.CustomerOrders.Count);
```

Execute a stored procedure without a result set.
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

    insertedRecords = conn.Procedure()
        .AddSqlParameter(idParam)
        .AddSqlParameter("@City", customer.City)
        .AddSqlParameter("@Country", customer.Country)
        .AddSqlParameter("@FirstName", customer.FirstName)
        .AddSqlParameter("@LastName", customer.LastName)
        .AddSqlParameter("@Phone", customer.Phone)
        .ExecuteNonQuery(conn, "dbo.SaveCustomer");

    int outputId = idParam.GetValueOrDefault<int>();
}

Assert.IsTrue(insertedRecords > 0);
```


###Performance
Internally, SprocMapper will cache property members for quick lookups and uses FastMember 
for assigning values. Performance is more dependant on how well your SQL is written, indexes, hardware, etc. 
Please feel free to blog, compare and review.


