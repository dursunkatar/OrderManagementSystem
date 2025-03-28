using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMS.Application.DTOs;
using OMS.Application.Interfaces;
using System.Security.Claims;

namespace OMS.API.Controllers
{
    [Authorize(Roles = "Customer")]
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Geçersiz kullanıcı" });
                }

                var cart = await _cartService.GetCartAsync(userId);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepet getirilirken hata oluştu");
                return StatusCode(500, new { message = "Sepet getirilirken bir hata oluştu" });
            }
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Geçersiz kullanıcı" });
                }

                var cart = await _cartService.AddToCartAsync(userId, request);
                return Ok(cart);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Sepete ürün eklenirken hata: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepete ürün eklenirken hata oluştu");
                return StatusCode(500, new { message = "Sepete ürün eklenirken bir hata oluştu" });
            }
        }

        [HttpPut("items")]
        public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Geçersiz kullanıcı" });
                }

                var cart = await _cartService.UpdateCartItemAsync(userId, request);
                return Ok(cart);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Sepetteki ürün güncellenirken hata: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepetteki ürün güncellenirken hata oluştu");
                return StatusCode(500, new { message = "Sepetteki ürün güncellenirken bir hata oluştu" });
            }
        }

        [HttpDelete("items/{productId}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Geçersiz kullanıcı" });
                }

                var result = await _cartService.RemoveFromCartAsync(userId, productId);
                if (result)
                {
                    return Ok(new { message = "Ürün sepetten kaldırıldı" });
                }
                return NotFound(new { message = "Ürün sepette bulunamadı" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün sepetten kaldırılırken hata oluştu");
                return StatusCode(500, new { message = "Ürün sepetten kaldırılırken bir hata oluştu" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Geçersiz kullanıcı" });
                }

                var result = await _cartService.ClearCartAsync(userId);
                if (result)
                {
                    return Ok(new { message = "Sepet temizlendi" });
                }
                return NotFound(new { message = "Sepet bulunamadı" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepet temizlenirken hata oluştu");
                return StatusCode(500, new { message = "Sepet temizlenirken bir hata oluştu" });
            }
        }

        // JWT token'dan kullanıcı ID'sini alma yardımcı metodu
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }
    }
}
