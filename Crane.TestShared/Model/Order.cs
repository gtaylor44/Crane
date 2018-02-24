using Crane.TestShared.Type;
using System;
using System.Collections.Generic;

namespace Crane.TestCommon.Model
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderNumber { get; set; }
        public int CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderType OrderType { get; set; }
        public ICollection<OrderItem> OrderItemList { get; set; }      
    }
}
