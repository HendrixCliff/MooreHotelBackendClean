using MooreHotelAndSuites.Domain.Enums;


namespace MooreHotelAndSuites.Domain.Entities
{
    public class Reservation
{
    public Guid Id { get; set; }

    public Guid RoomId { get; set; }
    public Room Room { get; set; } = default!;

    public Guid UserId { get; set; }

    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }

    public ReservationStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
}

}