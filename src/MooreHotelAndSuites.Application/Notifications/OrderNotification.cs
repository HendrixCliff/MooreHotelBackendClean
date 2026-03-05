using MooreHotelAndSuites.Domain.Enums;

namespace MooreHotelAndSuites.Application.Notifications
{
    public class OrderNotification
    {
        public Guid OrderId { get; set; }
        public NotificationChannel Channel { get; set; }
        public string CustomerName { get; set; } = string.Empty;  // Add this
        public string? RoomNumber { get; set; }                     // Add this
        public string Message { get; set; } = string.Empty;
    }
}