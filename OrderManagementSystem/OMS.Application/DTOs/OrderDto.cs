namespace OMS.Application.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        public decimal TotalAmount { get; set; }

        public int ItemCount => Items?.Count ?? 0;

        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }
}
