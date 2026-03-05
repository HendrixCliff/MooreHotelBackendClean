namespace MooreHotelAndSuites.Application.DTOs.Booking
{
    public sealed class CreateBookingRequestDto
{
    public List<Guid> RoomIds { get; init; } = new();
    public DateTime CheckInDate { get; init; }
    public DateTime CheckOutDate { get; init; }
    public List<int> OccupantsPerRoom { get; set; } = new();
    public int Occupants { get; init; } = 1;
    public string? GuestFullName { get; init; }
    public string? GuestPhoneNumber { get; init; }
    public string? GuestEmail { get; init; }
}
}
