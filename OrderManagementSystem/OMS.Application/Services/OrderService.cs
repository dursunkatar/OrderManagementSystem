using OMS.Application.DTOs;
using OMS.Application.Interfaces;

namespace OMS.Application.Services
{
    public class OrderService : IOrderService
    {
        public Task<OrderDto> CancelOrderAsync(Guid orderId)
        {
            throw new NotImplementedException();
        }

        public Task<OrderDto> CompleteOrderAsync(Guid orderId)
        {
            throw new NotImplementedException();
        }

        public Task<OrderDto> CreateOrderAsync(CreateOrderRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResult<OrderDto>> GetCustomerOrdersAsync(string customerId, int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<OrderDto> GetOrderAsync(Guid orderId)
        {
            throw new NotImplementedException();
        }
    }
}
