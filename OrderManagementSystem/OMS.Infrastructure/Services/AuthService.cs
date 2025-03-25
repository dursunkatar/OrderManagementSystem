using Microsoft.Extensions.Configuration;
using OMS.Application.DTOs;
using OMS.Application.Interfaces;
using OMS.Domain.Entities;
using OMS.Domain.Helpers;
using System.Security.Cryptography;
using System.Text;


namespace OMS.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AuthService(
            ICustomerRepository customerRepository,
            IUnitOfWork unitOfWork,
            IConfiguration configuration)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<UserDto> ValidateUserAsync(string email, string password)
        {
            var customer = await _customerRepository.GetByEmailAsync(email);
            if (customer == null)
                return null;

            if (SecurityHelper.VerifyMD5Hash(password, customer.PasswordHash))
            {
                return new UserDto
                {
                    Id = customer.Id,
                    Email = customer.Email,
                    FullName = customer.FullName
                };
            }

            return null;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            var customer = await _customerRepository.GetByEmailAsync(email);
            return customer != null;
        }

        public async Task<int> CreateUserAsync(RegisterRequest request)
        {
            var customer = Customer.Create(
                request.Email,
                request.FirstName,
                request.LastName,
                request.PhoneNumber,
                request.Address,
                request.Password);

            await _customerRepository.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            return customer.Id;
        }

        public Task<bool> AssignRoleAsync(int userId, string roleName)
        {
            // TODO: doldurulacak
            // Müşteriye rol atama implementasyonu
            // Bu, veritabanınızdaki rol yapısına bağlıdır
            return Task.FromResult(true);
        }

        public Task<IEnumerable<string>> GetUserRolesAsync(int userId)
        {
            // TODO: doldurulacak
            // Müşteri rollerini getirme implementasyonu
            // Bu, veritabanınızdaki rol yapısına bağlıdır
            return Task.FromResult<IEnumerable<string>>(new[] { "Customer" });
        }
    }
}
