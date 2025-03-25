using System.ComponentModel.DataAnnotations;

namespace OMS.Application.DTOs
{
    public class CreditCardInfo
    {
        [Required]
        [StringLength(100)]
        public string CardholderName { get; set; }
        
        [Required]
        [CreditCard]
        public string CardNumber { get; set; }
        
        [Required]
        [Range(1, 12)]
        public int ExpiryMonth { get; set; }
       
        [Required]
        public int ExpiryYear { get; set; }

        [Required]
        [StringLength(4, MinimumLength = 3)]
        public string SecurityCode { get; set; }
    }
}