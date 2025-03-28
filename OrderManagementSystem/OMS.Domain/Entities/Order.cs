using System.ComponentModel.DataAnnotations.Schema;

namespace OMS.Domain.Entities
{
    [Table("Orders")]
    public class Order
    {
        public int Id { get; private set; }
        public int CustomerId { get; private set; }
        public int StatusId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public DateTime? CancelledAt { get; private set; }
        public ICollection<OrderItem> Items { get; private set; }

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [ForeignKey("StatusId")]
        public OrderStatus Status { get; private set; }

        public string ShippingAddress { get; set; }

        public decimal TotalAmount => Items.Sum(i => i.Price * i.Quantity);

        public static Order Create(int customerId, string shippingAddress, IEnumerable<OrderItem> items)
        {
            return new Order
            {
                CustomerId = customerId,
                CreatedAt = DateTime.UtcNow,
                Items = items.ToList(),
                StatusId = Const.OrderStatus.PENDING,
                ShippingAddress = shippingAddress,
            };
        }

        public void Complete()
        {
            StatusId = Const.OrderStatus.COMPLETED;
            CompletedAt = DateTime.UtcNow;
        }

        public void Cancel()
        {
            StatusId = Const.OrderStatus.CANCELLED;
            CancelledAt = DateTime.UtcNow;
        }
    }
}
