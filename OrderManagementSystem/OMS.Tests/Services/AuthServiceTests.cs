using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using OMS.Application.Interfaces;
using OMS.Domain.Entities;
using OMS.Infrastructure.Persistence;
using OMS.Infrastructure.Services;

namespace OMS.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<OrderDbContext> _orderDbContextMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _configurationMock = new Mock<IConfiguration>();

            _authService = new AuthService(
                _customerRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _configurationMock.Object,
                _orderDbContextMock.Object);
        }

        [Fact]
        public async Task ValidateUserAsync_WithValidCredentials_ReturnsUserDto()
        {
            var email = "test@example.com";
            var password = "password123";
            var customer = Customer.Create(email, "Test", "User", "5551234567", "Test Address", password);

            _customerRepositoryMock.Setup(repo => repo.GetByEmailAsync(email))
                .ReturnsAsync(customer);

            var result = await _authService.ValidateUserAsync(email, password);

            result.Should().NotBeNull();
            result.Email.Should().Be(email);
        }

        [Fact]
        public async Task ValidateUserAsync_WithInvalidCredentials_ReturnsNull()
        {
            var email = "test@example.com";
            var password = "password123";
            var wrongPassword = "wrongpassword";
            var customer = Customer.Create(email, "Test", "User", "5551234567", "Test Address", password);

            _customerRepositoryMock.Setup(repo => repo.GetByEmailAsync(email))
                .ReturnsAsync(customer);

            var result = await _authService.ValidateUserAsync(email, wrongPassword);

            result.Should().BeNull();
        }

        [Fact]
        public async Task EmailExistsAsync_WithExistingEmail_ReturnsTrue()
        {
            var email = "test@example.com";
            var customer = Customer.Create(email, "Test", "User", "5551234567", "Test Address", "password123");

            _customerRepositoryMock.Setup(repo => repo.GetByEmailAsync(email))
                .ReturnsAsync(customer);

            var result = await _authService.EmailExistsAsync(email);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task EmailExistsAsync_WithNonExistingEmail_ReturnsFalse()
        {
            var email = "nonexistent@example.com";

            _customerRepositoryMock.Setup(repo => repo.GetByEmailAsync(email))
                .ReturnsAsync((Customer)null);

            var result = await _authService.EmailExistsAsync(email);

            result.Should().BeFalse();
        }
    }
}
