using MooreHotelAndSuites.Domain.Events;
using MooreHotelAndSuites.Domain.Abstractions;
using MooreHotelAndSuites.Application.Notifications;
using MooreHotelAndSuites.Application.Interfaces.Repositories;
using MooreHotelAndSuites.Domain.Enums;

namespace MooreHotelAndSuites.Application.EventHandlers
{
public class OrderCreatedHandler : IDomainEventHandler<OrderCreatedEvent>
{
    private readonly NotificationRouter _router;

        public OrderCreatedHandler(NotificationRouter router)
    {
        _router = router;
    }

    public async Task HandleAsync(OrderCreatedEvent notification)
    {
        var orderNotification = new OrderNotification
        {
            OrderId = notification.OrderId,
            Channel = GetChannel(notification.Source),
            CustomerName = notification.CustomerName,
            Message = $"New {notification.Source} order from {notification.CustomerName}"
        };

        await _router.RouteAsync(orderNotification);
    }

    private static NotificationChannel GetChannel(OrderSource source) => source switch
    {
        OrderSource.Kitchen => NotificationChannel.Kitchen,
        OrderSource.Bar => NotificationChannel.Bar,
        OrderSource.RoomService => NotificationChannel.Kitchen,
        OrderSource.Laundry => NotificationChannel.Laundry,
        OrderSource.EventHall => NotificationChannel.EventService,
        _ => throw new ArgumentOutOfRangeException(nameof(source))
    };
}

}
