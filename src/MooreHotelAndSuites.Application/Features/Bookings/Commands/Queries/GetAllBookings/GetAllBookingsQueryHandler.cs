using MediatR;
using MooreHotelAndSuites.Application.DTOs.Booking;
using MooreHotelAndSuites.Application.Interfaces.Repositories;

namespace MooreHotelAndSuites.Application.Features.Bookings.Queries.GetAllBookings;

public sealed class GetAllBookingsQueryHandler
    : IRequestHandler<GetAllBookingsQuery, List<BookingDto>>
{
    private readonly IBookingRepository _repo;

    public GetAllBookingsQueryHandler(IBookingRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<BookingDto>> Handle(
        GetAllBookingsQuery request,
        CancellationToken cancellationToken)
    {
        var bookings = await _repo.GetAllAsync();

        return bookings.Select(b => new BookingDto
        {
            Id = b.Id,
            CheckIn = b.CheckIn,
            CheckOut = b.CheckOut,
            RoomId = b.RoomId,
            GuestId = b.GuestId,
            Occupants = b.Occupants,
            Status = b.Status
        }).ToList();
    }
}