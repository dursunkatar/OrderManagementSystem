using System.ComponentModel.DataAnnotations;

namespace OMS.Application.DTOs
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur")]
        public string Password { get; set; }
    }
}
