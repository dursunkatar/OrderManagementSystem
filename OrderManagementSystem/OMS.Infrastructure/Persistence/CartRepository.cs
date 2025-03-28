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
    public class CartRepository : ICartRepository
    {
        private readonly OrderDbContext _context;

        public CartRepository(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<Cart> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }

        public async Task<Cart> AddAsync(Cart cart)
        {
            await _context.Carts.AddAsync(cart);
            return cart;
        }

        public async Task UpdateAsync(Cart cart)
        {
            _context.Entry(cart).State = EntityState.Modified;

            foreach (var item in cart.Items)
            {
                if (item.Id == 0)
                {
                    _context.Entry(item).State = EntityState.Added;
                }
                else
                {
                    _context.Entry(item).State = EntityState.Modified;
                }
            }
        }

        public async Task ClearCartAsync(int customerId)
        {
            var cart = await GetByCustomerIdAsync(customerId);
            if (cart != null)
            {
                var items = _context.Set<CartItem>().Where(i => i.CartId == cart.Id);
                _context.RemoveRange(items);
            }
        }
    }
}
