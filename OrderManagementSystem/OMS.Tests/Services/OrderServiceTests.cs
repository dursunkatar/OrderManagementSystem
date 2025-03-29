using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OMS.Application.DTOs;
using OMS.Application.Interfaces;
using OMS.Application.Services;
using OMS.Domain.Entities;

namespace OMS.Tests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly Mock<ICartRepository> _cartRepositoryMock;

        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<OrderService>> _loggerMock;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _productRepositoryMock = new Mock<IProductRepository>();
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _cacheServiceMock = new Mock<ICacheService>();
            _cartRepositoryMock = new Mock<ICartRepository>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<OrderService>>();

            _orderService = new OrderService(
                _orderRepositoryMock.Object,
                _productRepositoryMock.Object,
                _customerRepositoryMock.Object,
                _cacheServiceMock.Object,
                _eventPublisherMock.Object,
                _unitOfWorkMock.Object,
                _loggerMock.Object,
                _cartRepositoryMock.Object);
        }

        [Fact]
        public async Task GetOrderAsync_WithValidId_ReturnsOrder()
        {
            // Arrange
            var orderId = 1;
            var customerId = 1;
            var customer = Customer.Create("test@hotmail.com", "test", "testlastname", "5134541", "asdasd", "12345"); ;
            var status = new OrderStatus { Id = 0, Name = "Pending" };
            var orderItems = new List<OrderItem>();  // OrderItem listesi oluşturun

            var order = new Order();  // Order nesnesi oluşturun ve gerekli özellikleri ayarlayın

            _orderRepositoryMock.Setup(repo => repo.GetByIdAsync(orderId))
                .ReturnsAsync(order);

            // Act
            var result = await _orderService.GetOrderAsync(orderId);

            // Assert
            result.Should().NotBeNull();
            _orderRepositoryMock.Verify(repo => repo.GetByIdAsync(orderId), Times.Once);
        }

        [Fact]
        public async Task GetOrderAsync_WithCachedOrder_ReturnsCachedOrder()
        {
            // Arrange
            var orderId = 1;
            var cachedOrder = new OrderDto { Id = orderId };

            _cacheServiceMock.Setup(cache => cache.GetAsync<OrderDto>(It.IsAny<string>()))
                .ReturnsAsync(cachedOrder);

            // Act
            var result = await _orderService.GetOrderAsync(orderId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeSameAs(cachedOrder);
            _orderRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetCustomerOrdersAsync_ShouldPaginateResults()
        {
            // Arrange
            var customerId = 1;
            var page = 2;
            var pageSize = 10;
            var totalCount = 25;
            var orders = new List<Order> { new Order(), new Order() };  // Test için 2 sipariş

            _orderRepositoryMock.Setup(repo => repo.GetByCustomerIdAsync(customerId, page, pageSize))
                .ReturnsAsync(orders);
            _orderRepositoryMock.Setup(repo => repo.GetCustomerOrderCountAsync(customerId))
                .ReturnsAsync(totalCount);

            // Act
            var result = await _orderService.GetCustomerOrdersAsync(customerId, page, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.Page.Should().Be(page);
            result.PageSize.Should().Be(pageSize);
            result.TotalCount.Should().Be(totalCount);
            result.TotalPages.Should().Be(3);  // 25 / 10 = 2.5 -> 3 sayfa
        }

        [Fact]
        public async Task CompleteOrderAsync_WithPendingOrder_CompletesOrder()
        {
            // Arrange
            var orderId = 1;
            var order = new Order();  // Order nesnesi oluşturun, StatusId'yi Pending olarak ayarlayın

            _orderRepositoryMock.Setup(repo => repo.GetByIdAsync(orderId))
                .ReturnsAsync(order);

            // Act
            var result = await _orderService.CompleteOrderAsync(orderId);

            // Assert
            result.Should().NotBeNull();
            _orderRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Order>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
            _eventPublisherMock.Verify(pub => pub.PublishAsync(It.IsAny<object>()), Times.Once);
        }
    }
}