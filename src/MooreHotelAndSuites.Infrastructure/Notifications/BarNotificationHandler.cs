using MooreHotelAndSuites.Application.Notifications;
using MooreHotelAndSuites.Application.Interfaces.Services;
using MooreHotelAndSuites.Domain.Enums;

namespace MooreHotelAndSuites.Infrastructure.Notifications
{
    public class BarNotificationHandler : INotificationChannelHandler
    {
        private readonly INotificationService _notify;
        public NotificationChannel Channel => NotificationChannel.Bar;

        public BarNotificationHandler(INotificationService notify)
        {
            _notify = notify;
        }

      public async Task HandleAsync(OrderNotification n)
{
    await _notify.BroadcastAsync("bar", new
    {
        type = "NEW_ORDER",
        orderId = n.OrderId,
        customerName = n.CustomerName,
        roomNumber = n.RoomNumber,
        message = $"New order from {n.CustomerName}" + 
                 (n.RoomNumber != null ? $" - Room {n.RoomNumber}" : "")
    });
}
    }
}
