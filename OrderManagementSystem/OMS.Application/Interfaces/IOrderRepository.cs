using OMS.Domain.Entities;

namespace OMS.Application.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> GetByIdAsync(Guid id);
        Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId, int page, int pageSize);
        Task<Order> AddAsync(Order order);
        Task UpdateAsync(Order order);
    }
}
