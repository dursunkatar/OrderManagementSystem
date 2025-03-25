using System.Security.Claims;

namespace OMS.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateJwtToken(string userId, string email, IEnumerable<string> roles = null);
        ClaimsPrincipal ValidateToken(string token);
    }
}
