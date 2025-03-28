using Microsoft.EntityFrameworkCore;
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

        public OrderRepository(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<Order> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Customer)
                .Include(o => o.Status)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId, int page, int pageSize)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Status)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetCustomerOrderCountAsync(int customerId)
        {
            return await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .CountAsync();
        }

        public async Task<Order> AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            return order;
        }

        public async Task UpdateAsync(Order order)
        {
            _context.Entry(order).State = EntityState.Modified;
        }
    }
}
