using OMS.Domain.Entities;

namespace OMS.Application.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Order> GetByIdAsync(int id);
        Task<Customer> GetByEmailAsync(string email);
        Task<IEnumerable<Customer>> GetByCustomerIdAsync(int customerId, int page, int pageSize);
        Task<int> GetCustomerOrderCountAsync(int customerId);
        Task<Customer> AddAsync(Customer customer);
        Task UpdateAsync(Customer customer);
    }
}
