using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OMS.Application.DTOs;
using OMS.Application.Interfaces;
using System.Security.Claims;

namespace OMS.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IOrderService orderService,
            ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpPost("from-cart")]
        public async Task<IActionResult> CreateOrderFromCart([FromBody] CreateOrderFromCartRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // JWT token'dan kullanıcı ID'sini al
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Geçersiz kullanıcı" });
                }


                // Sepetten sipariş oluştur
                var orderDto = await _orderService.CreateOrderFromCartAsync(request, userId);

                return CreatedAtAction(nameof(GetOrder), new { id = orderDto.Id }, orderDto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Sepetten sipariş oluşturulurken doğrulama hatası: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepetten sipariş oluşturulurken hata oluştu");
                return StatusCode(500, new { message = "Sepetten sipariş oluşturulurken bir hata oluştu" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Geçersiz kullanıcı" });
                }

                var orderDto = await _orderService.GetOrderAsync(id);
                if (orderDto == null)
                {
                    return NotFound(new { message = "Sipariş bulunamadı" });
                }

                // Kullanıcı sadece kendi siparişlerini görebilir (admin olmadığı sürece)
                if (orderDto.CustomerId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                return Ok(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş getirilirken hata oluştu");
                return StatusCode(500, new { message = "Sipariş getirilirken bir hata oluştu" });
            }
        }

        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Geçersiz kullanıcı" });
                }

                var pagedOrders = await _orderService.GetCustomerOrdersAsync(userId, page, pageSize);
                return Ok(pagedOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Siparişler getirilirken hata oluştu");
                return StatusCode(500, new { message = "Siparişler getirilirken bir hata oluştu" });
            }
        }

        [HttpGet("customer/{customerId}")]
        [Authorize(Roles = "Admin")] // Sadece admin kullanıcılar diğer müşterilerin siparişlerini görebilir
        public async Task<IActionResult> GetCustomerOrders(int customerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var pagedOrders = await _orderService.GetCustomerOrdersAsync(customerId, page, pageSize);
                return Ok(pagedOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Müşteri siparişleri getirilirken hata oluştu");
                return StatusCode(500, new { message = "Müşteri siparişleri getirilirken bir hata oluştu" });
            }
        }

        [HttpPut("{id}/complete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CompleteOrder(int id)
        {
            try
            {
                var orderDto = await _orderService.CompleteOrderAsync(id);
                return Ok(orderDto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Sipariş tamamlanırken doğrulama hatası: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş tamamlanırken hata oluştu");
                return StatusCode(500, new { message = "Sipariş tamamlanırken bir hata oluştu" });
            }
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                // Siparişi kontrol et
                var order = await _orderService.GetOrderAsync(id);
                if (order == null)
                {
                    return NotFound(new { message = "Sipariş bulunamadı" });
                }

                var userId = GetCurrentUserId();

                // Kullanıcı sadece kendi siparişlerini iptal edebilir (admin olmadığı sürece)
                if (order.CustomerId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var orderDto = await _orderService.CancelOrderAsync(id);
                return Ok(orderDto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Sipariş iptal edilirken doğrulama hatası: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş iptal edilirken hata oluştu");
                return StatusCode(500, new { message = "Sipariş iptal edilirken bir hata oluştu" });
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
