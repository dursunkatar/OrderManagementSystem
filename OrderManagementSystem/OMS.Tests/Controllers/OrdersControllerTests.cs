using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OMS.API.Controllers;
using OMS.Application.DTOs;
using OMS.Application.Interfaces;
using System.Security.Claims;

namespace OMS.Tests.Controllers
{
    public class OrdersControllerTests
    {
        private readonly Mock<IOrderService> _orderServiceMock;
        private readonly Mock<ICartService> _cartServiceMock;
        private readonly Mock<ILogger<OrdersController>> _loggerMock;
        private readonly OrdersController _controller;

        public OrdersControllerTests()
        {
            _orderServiceMock = new Mock<IOrderService>();
            _cartServiceMock = new Mock<ICartService>();
            _loggerMock = new Mock<ILogger<OrdersController>>();

            _controller = new OrdersController(
                _orderServiceMock.Object,
                _loggerMock.Object);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = principal
                }
            };
        }

        [Fact]
        public async Task GetOrder_WithValidId_ReturnsOkResult()
        {
            var orderId = 1;
            var orderDto = new OrderDto { Id = orderId, CustomerId = 1 };

            _orderServiceMock.Setup(service => service.GetOrderAsync(orderId))
                .ReturnsAsync(orderDto);

            var result = await _controller.GetOrder(orderId);
         
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedOrder = okResult.Value.Should().BeAssignableTo<OrderDto>().Subject;
            returnedOrder.Id.Should().Be(orderId);
        }

        [Fact]
        public async Task GetOrder_WithInvalidId_ReturnsNotFound()
        {
            var orderId = 999;
            OrderDto nullOrder = null;

            _orderServiceMock.Setup(service => service.GetOrderAsync(orderId))
                .ReturnsAsync(nullOrder);

            var result = await _controller.GetOrder(orderId);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task CreateOrder_WithValidRequest_ReturnsCreatedResult()
        {
            var request = new CreateOrderFromCartRequest
            {
                ShippingAddress = "adress",
                Payment = new PaymentRequest
                {
                    PaymentMethod = "CreditCard"
                }
            };

            var orderDto = new OrderDto { Id = 1 };
            const int customerId = 2;

            _orderServiceMock.Setup(service => service.CreateOrderFromCartAsync(It.IsAny<CreateOrderFromCartRequest>(), customerId))
                .ReturnsAsync(orderDto);

            var result = await _controller.CreateOrderFromCart(request);
            
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(OrdersController.GetOrder));
            createdResult.RouteValues["id"].Should().Be(orderDto.Id);
            createdResult.Value.Should().Be(orderDto);
        }

        [Fact]
        public async Task CancelOrder_WithValidId_ReturnsOkResult()
        {
            
            var orderId = 1;
            var orderDto = new OrderDto { Id = orderId, CustomerId = 1 };

            _orderServiceMock.Setup(service => service.GetOrderAsync(orderId))
                .ReturnsAsync(orderDto);
            _orderServiceMock.Setup(service => service.CancelOrderAsync(orderId))
                .ReturnsAsync(orderDto);

            
            var result = await _controller.CancelOrder(orderId);

            
            result.Should().BeOfType<OkObjectResult>();
            _orderServiceMock.Verify(service => service.CancelOrderAsync(orderId), Times.Once);
        }
    }
}
