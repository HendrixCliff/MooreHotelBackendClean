using MooreHotelAndSuites.Application.Notifications;
using MooreHotelAndSuites.Application.Interfaces.Services;
using MooreHotelAndSuites.Domain.Enums;

namespace MooreHotelAndSuites.Infrastructure.Notifications
{
    public class KitchenNotificationHandler : INotificationChannelHandler
    {
        private readonly INotificationService _notify;
        public NotificationChannel Channel => NotificationChannel.Kitchen;

        public KitchenNotificationHandler(INotificationService notify)
        {
            _notify = notify;
        }

        public async Task HandleAsync(OrderNotification n)
        {
            await _notify.BroadcastAsync("kitchen", new
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