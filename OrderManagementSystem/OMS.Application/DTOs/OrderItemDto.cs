namespace OMS.Application.DTOs
{
    public class OrderItemDto
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
       
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductImageUrl { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public decimal TotalPrice => Price * Quantity;
    }
}
