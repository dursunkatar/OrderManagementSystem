using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMS.Application.DTOs;
using OMS.Application.Interfaces;

namespace OMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            ITokenService tokenService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _authService.ValidateUserAsync(request.Email, request.Password);
                if (user == null)
                {
                    return Unauthorized(new { message = "Geçersiz e-posta adresi veya şifre" });
                }

                // Kullanıcı rollerini al (örneğin: "Admin", "Customer" vb.)
                var roles = await _authService.GetUserRolesAsync(user.Id);

                // JWT token oluştur
                var token = _tokenService.GenerateJwtToken(user.Id.ToString(), user.Email, roles);

                return Ok(new
                {
                    token,
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        fullName = user.FullName
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login sırasında hata oluştu");
                return StatusCode(500, new { message = "Giriş işlemi sırasında bir hata oluştu" });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // E-posta adresi kullanılıyor mu?
                if (await _authService.EmailExistsAsync(request.Email))
                {
                    return Conflict(new { message = "Bu e-posta adresi zaten kullanılmaktadır" });
                }

                // Yeni kullanıcı oluştur
                var userId = await _authService.CreateUserAsync(request);
                if (userId == default)
                {
                    return BadRequest(new { message = "Kullanıcı oluşturulamadı" });
                }

                // Varsayılan olarak "Customer" rolü ata
                await _authService.AssignRoleAsync(userId, "Customer");

                // Yeni oluşturulan kullanıcı için token oluştur
                var token = _tokenService.GenerateJwtToken(userId.ToString(), request.Email, new[] { "Customer" });

                return Ok(new
                {
                    token,
                    user = new
                    {
                        id = userId,
                        email = request.Email,
                        fullName = $"{request.FirstName} {request.LastName}"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kayıt sırasında hata oluştu");
                return StatusCode(500, new { message = "Kayıt işlemi sırasında bir hata oluştu" });
            }
        }
    }
}
