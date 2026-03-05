using MediatR;
using MooreHotelAndSuites.Application.DTOs.Booking;
using MooreHotelAndSuites.Application.Interfaces.Repositories;

namespace MooreHotelAndSuites.Application.Features.Bookings.Queries.GetBookingById;

public sealed class GetBookingByIdQueryHandler
    : IRequestHandler<GetBookingByIdQuery, BookingDto?>
{
    private readonly IBookingRepository _repo;

    public GetBookingByIdQueryHandler(IBookingRepository repo)
    {
        _repo = repo;
    }

    public async Task<BookingDto?> Handle(
        GetBookingByIdQuery request,
        CancellationToken cancellationToken)
    {
        var booking = await _repo.GetByIdAsync(request.BookingId);

        if (booking == null)
            return null;

        return new BookingDto
        {
            Id = booking.Id,
            CheckIn = booking.CheckIn,
            CheckOut = booking.CheckOut,
            RoomId = booking.RoomId,
            GuestId = booking.GuestId,
            Occupants = booking.Occupants,
            Status = booking.Status
        };
    }
}