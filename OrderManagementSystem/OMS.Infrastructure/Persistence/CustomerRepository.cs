using Microsoft.EntityFrameworkCore;
using OMS.Application.Interfaces;
using OMS.Domain.Entities;

namespace OMS.Infrastructure.Persistence
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly OrderDbContext _context;

        public CustomerRepository(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<Customer> GetByIdAsync(int id)
        {
            return await _context.Customers.FindAsync(id);
        }

        public async Task<Customer> GetByEmailAsync(string email)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower());
        }

        public async Task<IEnumerable<Customer>> GetByCustomerIdAsync(int customerId, int page, int pageSize)
        {
            return await _context.Customers
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

        public async Task<Customer> AddAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
            return customer;
        }

        public async Task UpdateAsync(Customer customer)
        {
            _context.Entry(customer).State = EntityState.Modified;
        }
    }
}
