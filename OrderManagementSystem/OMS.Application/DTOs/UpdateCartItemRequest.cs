using System.ComponentModel.DataAnnotations;

namespace OMS.Application.DTOs
{
    public class UpdateCartItemRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(0, 1000, ErrorMessage = "Miktar 0 ile 1000 arasında olmalıdır.")]
        public int Quantity { get; set; }
    }
}
