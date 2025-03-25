using OMS.Domain.Entities;


namespace OMS.Application.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetListByProductIdsAsync(int[] productIds);
    }
}
