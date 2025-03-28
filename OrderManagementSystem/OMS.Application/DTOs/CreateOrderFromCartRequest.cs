using System.ComponentModel.DataAnnotations;

namespace OMS.Application.DTOs
{
    public class CreateOrderFromCartRequest
    {
        [Required]
        public string ShippingAddress { get; set; }

        [Required]
        public PaymentRequest Payment { get; set; }

        public string CustomerNote { get; set; }
    }
}
