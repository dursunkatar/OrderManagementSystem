using System.ComponentModel.DataAnnotations.Schema;

namespace OMS.Domain.Entities
{
    [Table("OrderItems")]
    public class OrderItem
    {
        public int Id { get; private set; }
        public int OrderId { get; private set; }
        public int ProductId { get; private set; }
        public string ProductName { get; private set; }
        public decimal Price { get; private set; }
        public int Quantity { get; private set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        public static OrderItem Create(int productId, string productName, decimal price, int quantity)
        {
            return new OrderItem
            {
                ProductId = productId,
                ProductName = productName,
                Price = price,
                Quantity = quantity
            };
        }
    }
}
