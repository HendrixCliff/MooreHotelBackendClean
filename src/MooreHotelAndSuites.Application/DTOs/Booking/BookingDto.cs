using MooreHotelAndSuites.Domain.Enums;

namespace MooreHotelAndSuites.Application.DTOs.Booking
{
    public class BookingDto
    {
        public Guid Id { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int Occupants { get; set; }
        public Guid RoomId { get; set; }
        public int GuestId { get; set; }

        public BookingStatus Status { get; set; }

        public decimal Amount { get; set; } 
        public string? GroupReference { get; set; }
    }
}