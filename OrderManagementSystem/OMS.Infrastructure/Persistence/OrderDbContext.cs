using Microsoft.EntityFrameworkCore;
using OMS.Domain.Entities;

namespace OMS.Infrastructure.Persistence
{
    public class OrderDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurations...
            base.OnModelCreating(modelBuilder);
        }
    }
}
