using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OMS.Application.DTOs;
using OMS.Application.Interfaces;

namespace OMS.API.Controllers
{
    [Authorize]
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

        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetCart(int customerId)
        {
            try
            {
                var cart = await _cartService.GetCartAsync(customerId);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepet getirilirken hata oluştu");
                return StatusCode(500, new { message = "Sepet getirilirken bir hata oluştu" });
            }
        }

        [HttpPost("{customerId}/items")]
        public async Task<IActionResult> AddToCart(int customerId, [FromBody] AddToCartRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var cart = await _cartService.AddToCartAsync(customerId, request);
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

        [HttpPut("{customerId}/items")]
        public async Task<IActionResult> UpdateCartItem(int customerId, [FromBody] UpdateCartItemRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var cart = await _cartService.UpdateCartItemAsync(customerId, request);
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

        [HttpDelete("{customerId}/items/{productId}")]
        public async Task<IActionResult> RemoveFromCart(int customerId, int productId)
        {
            try
            {
                var result = await _cartService.RemoveFromCartAsync(customerId, productId);
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

        [HttpDelete("{customerId}")]
        public async Task<IActionResult> ClearCart(int customerId)
        {
            try
            {
                var result = await _cartService.ClearCartAsync(customerId);
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
    }
}
