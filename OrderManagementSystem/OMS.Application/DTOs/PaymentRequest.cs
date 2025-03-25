using System.ComponentModel.DataAnnotations;

namespace OMS.Application.DTOs
{
    public class PaymentRequest
    {
        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        public CreditCardInfo CreditCardInfo { get; set; }
    }
}
