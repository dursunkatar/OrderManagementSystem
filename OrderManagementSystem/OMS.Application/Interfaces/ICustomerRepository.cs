using OMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Application.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Order> GetByIdAsync(int id);
        Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId, int page, int pageSize);
        Task<int> GetCustomerOrderCountAsync(int customerId);
        Task<Order> AddAsync(Order order);
        Task UpdateAsync(Order order);
    }
}
