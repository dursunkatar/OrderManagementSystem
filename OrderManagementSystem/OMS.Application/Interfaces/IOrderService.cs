using OMS.Application.DTOs;

namespace OMS.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(CreateOrderRequest request);
        Task<OrderDto> GetOrderAsync(Guid orderId);
        Task<PagedResult<OrderDto>> GetCustomerOrdersAsync(string customerId, int page, int pageSize);
        Task<OrderDto> CompleteOrderAsync(Guid orderId);
        Task<OrderDto> CancelOrderAsync(Guid orderId);
    }
}
