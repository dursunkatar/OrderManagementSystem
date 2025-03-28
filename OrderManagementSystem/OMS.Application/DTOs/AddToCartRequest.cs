using System.ComponentModel.DataAnnotations;

namespace OMS.Application.DTOs
{
    public class AddToCartRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Miktar 1 ile 1000 arasında olmalıdır.")]
        public int Quantity { get; set; }
    }
}
