using System.ComponentModel.DataAnnotations;

namespace OMS.Application.DTOs
{
    public class AddressRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string FullName { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 5)]
        public string AddressLine1 { get; set; }


        [StringLength(200)]
        public string AddressLine2 { get; set; }

        [Required]
        [StringLength(100)]
        public string District { get; set; }

        [Required]
        [StringLength(50)]
        public string City { get; set; }

        [Required]
        [StringLength(20)]
        public string PostalCode { get; set; }

        [Required]
        [StringLength(50)]
        public string Country { get; set; }

        [Required]
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; }
    }
}