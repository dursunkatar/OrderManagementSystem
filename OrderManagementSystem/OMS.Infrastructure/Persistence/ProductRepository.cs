using Microsoft.EntityFrameworkCore;
using OMS.Application.Interfaces;
using OMS.Domain.Entities;

namespace OMS.Infrastructure.Persistence
{
    public class ProductRepository : IProductRepository
    {
        private readonly OrderDbContext _context;

        public ProductRepository(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<IEnumerable<Product>> GetListByProductIdsAsync(int[] productIds)
        {
            return await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Entry(product).State = EntityState.Modified;
        }
      
    }
}
