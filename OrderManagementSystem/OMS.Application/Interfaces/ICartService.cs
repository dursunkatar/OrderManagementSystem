using OMS.Application.DTOs;

namespace OMS.Application.Interfaces
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(int customerId);
        Task<CartDto> AddToCartAsync(int customerId, AddToCartRequest request);
        Task<CartDto> UpdateCartItemAsync(int customerId, UpdateCartItemRequest request);
        Task<bool> RemoveFromCartAsync(int customerId, int productId);
        Task<bool> ClearCartAsync(int customerId);
    }
}
