
using MooreHotelAndSuites.Domain.Abstractions;
using MooreHotelAndSuites.Domain.Enums;

namespace MooreHotelAndSuites.Domain.Events
{
    public class OrderCreatedEvent : IDomainEvent
    {
        public Guid OrderId { get; }
        public OrderSource Source { get; }
        public string CustomerName { get; }
        public DateTime OccurredOn { get; }

        public OrderCreatedEvent(Guid id, OrderSource source, string customerName)
        {
            OrderId = id;
            Source = source;
            CustomerName = customerName;
            OccurredOn = DateTime.UtcNow;
        }
    }
}