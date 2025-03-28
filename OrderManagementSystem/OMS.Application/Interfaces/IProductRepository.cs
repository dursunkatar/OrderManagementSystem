using OMS.Domain.Entities;

namespace OMS.Application.Interfaces
{
    public interface IProductRepository
    {
        Task<Product> GetByIdAsync(int id);
        Task UpdateAsync(Product product);
        Task<IEnumerable<Product>> GetListByProductIdsAsync(int[] productIds);
    }
}
