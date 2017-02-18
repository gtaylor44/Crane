namespace SprocMapperLibrary.TestCommon.Model
{
    public class OrderItem
    {
        public int OrderId { get; set; }
        public int Id { get; set; }     
        public int ProductId { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public Product Product { get; set; }
    }
}
