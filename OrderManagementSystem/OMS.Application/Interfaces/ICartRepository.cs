using OMS.Domain.Entities;

namespace OMS.Application.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart> GetByCustomerIdAsync(int customerId);
        Task<Cart> AddAsync(Cart cart);
        Task UpdateAsync(Cart cart);
        Task ClearCartAsync(int customerId);
    }
}
