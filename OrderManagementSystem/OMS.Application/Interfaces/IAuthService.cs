using OMS.Application.DTOs;

namespace OMS.Application.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto> ValidateUserAsync(string email, string password);
        Task<bool> EmailExistsAsync(string email);
        Task<int> CreateUserAsync(RegisterRequest request);
        Task<bool> AssignRoleAsync(int userId, string roleName);
        Task<IEnumerable<string>> GetUserRolesAsync(int userId);
        Task<bool> RemoveRoleAsync(int userId, string roleName);
        Task<bool> IsInRoleAsync(int userId, string roleName);
    }
}
