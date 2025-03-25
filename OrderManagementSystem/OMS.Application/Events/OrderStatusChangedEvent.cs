namespace OMS.Application.Events
{
    public class OrderStatusChangedEvent
    {
        public int OrderId { get; set; }
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
