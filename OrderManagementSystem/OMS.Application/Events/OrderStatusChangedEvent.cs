using OMS.Domain.Enums;

namespace OMS.Application.Events
{
    public class OrderStatusChangedEvent
    {
        public int OrderId { get; set; }
        public OrderStatus OldStatus { get; set; }
        public OrderStatus NewStatus { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
