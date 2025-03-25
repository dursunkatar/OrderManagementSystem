using OMS.Application.Interfaces;
using OMS.Domain.Entities;

namespace OMS.Infrastructure.Persistence
{
    public class ProductRepository : IProductRepository
    {
        public Task<IEnumerable<Product>> GetListByProductIdsAsync(int[] productIds)
        {
            throw new NotImplementedException();
        }
    }
}
