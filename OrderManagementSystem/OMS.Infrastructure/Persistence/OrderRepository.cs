using OMS.Application.Interfaces;
using OMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Infrastructure.Persistence
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _context;

        public Task<Order> AddAsync(Order order)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId, int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<Order> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetCustomerOrderCountAsync(int customerId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Order order)
        {
            throw new NotImplementedException();
        }
    }
}
