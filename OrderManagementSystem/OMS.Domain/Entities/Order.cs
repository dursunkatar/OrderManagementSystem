using OMS.Domain.Enums;

namespace OMS.Domain.Entities
{
    public class Order
    {
        public int Id { get; private set; }
        public int CustomerId { get; private set; }
        public OrderStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public DateTime? CancelledAt { get; private set; }
        public ICollection<OrderItem> Items { get; private set; }
        public decimal TotalAmount => Items.Sum(i => i.Price * i.Quantity);

        public static Order Create(int customerId, IEnumerable<OrderItem> items)
        {
            return new Order
            {
                CustomerId = customerId,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                Items = items.ToList()
            };
        }

        public void Complete()
        {
            Status = OrderStatus.Completed;
            CompletedAt = DateTime.UtcNow;
        }

        public void Cancel()
        {
            Status = OrderStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;
        }
    }
}
