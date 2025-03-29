using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OMS.Application.DTOs;
using OMS.Application.Interfaces;
using OMS.Domain.Entities;
using OMS.Domain.Helpers;
using OMS.Infrastructure.Persistence;

namespace OMS.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly OrderDbContext _context;

        public AuthService(
            ICustomerRepository customerRepository,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            OrderDbContext context)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _context = context;
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

        public async Task<bool> AssignRoleAsync(int userId, string roleName)
        {
            try
            {
                var user = await _customerRepository.GetByIdAsync(userId);
                if (user == null)
                    return false;

                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                if (role == null)
                    return false;

                
                var existingUserRole = await _context.UserRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);

                if (existingUserRole != null)
                    return true; 

                
                var userRole = new UserRole
                {
                    UserId = userId,
                    RoleId = role.Id
                };

                await _context.UserRoles.AddAsync(userRole);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> RemoveRoleAsync(int userId, string roleName)
        {
            try
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                if (role == null)
                    return false;

                var userRole = await _context.UserRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);

                if (userRole == null)
                    return false; 

                _context.UserRoles.Remove(userRole);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(int userId)
        {
            try
            {
                var roles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                    .Join(_context.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => r.Name)
                    .ToListAsync();

                return roles;
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<string>();
            }
        }

        public async Task<bool> IsInRoleAsync(int userId, string roleName)
        {
            try
            {
                return await _context.UserRoles
                    .AnyAsync(ur => ur.UserId == userId && ur.Role.Name == roleName);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
