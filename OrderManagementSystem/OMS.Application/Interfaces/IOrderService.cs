using OMS.Application.DTOs;

namespace OMS.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(CreateOrderRequest request);
        Task<OrderDto> GetOrderAsync(int orderId);
        Task<PagedResult<OrderDto>> GetCustomerOrdersAsync(int customerId, int page, int pageSize);
        Task<OrderDto> CompleteOrderAsync(int orderId);
        Task<OrderDto> CancelOrderAsync(int orderId);
        Task<OrderDto> CreateOrderFromCartAsync(CreateOrderFromCartRequest request);
    }
}
