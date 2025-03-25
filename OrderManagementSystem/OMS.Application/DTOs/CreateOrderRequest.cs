using System.ComponentModel.DataAnnotations;

namespace OMS.Application.DTOs
{
    public class CreateOrderRequest
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "En az bir ürün seçilmelidir.")]
        public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();

        [Required]
        public AddressRequest ShippingAddress { get; set; }

        [Required]
        public PaymentRequest Payment { get; set; }

        public string CustomerNote { get; set; }
    }

   
}
