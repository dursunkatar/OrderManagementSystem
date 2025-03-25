using System.ComponentModel.DataAnnotations;

namespace OMS.Application.DTOs
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Ad zorunludur")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ad en az 2, en fazla 50 karakter olmalıdır")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad zorunludur")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Soyad en az 2, en fazla 50 karakter olmalıdır")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor")]
        public string ConfirmPassword { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Adres zorunludur")]
        public string Address { get; set; }
    }
}
