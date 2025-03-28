using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OMS.Application.DTOs;
using OMS.Application.Interfaces;

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

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var orderDto = await _orderService.CreateOrderAsync(request);
                return CreatedAtAction(nameof(GetOrder), new { id = orderDto.Id }, orderDto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Sipariş oluşturulurken doğrulama hatası: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş oluşturulurken hata oluştu");
                return StatusCode(500, new { message = "Sipariş oluşturulurken bir hata oluştu" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            try
            {
                var orderDto = await _orderService.GetOrderAsync(id);
                if (orderDto == null)
                {
                    return NotFound(new { message = "Sipariş bulunamadı" });
                }
                return Ok(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş getirilirken hata oluştu");
                return StatusCode(500, new { message = "Sipariş getirilirken bir hata oluştu" });
            }
        }

        [HttpGet("customer/{customerId}")]
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
    }
}
