using MediatR;
using MooreHotelAndSuites.Application.DTOs.Booking;
using MooreHotelAndSuites.Application.Interfaces.Repositories;
using MooreHotelAndSuites.Domain.Enums;

public sealed class GetPendingBookingsHandler
    : IRequestHandler<GetPendingBookingsQuery, List<BookingDto>>
{
    private readonly IBookingRepository _repo;

    public GetPendingBookingsHandler(IBookingRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<BookingDto>> Handle(GetPendingBookingsQuery request, CancellationToken cancellationToken)
    {
        var pendingBookings = await _repo.GetAllByStatusAsync(BookingStatus.Pending);

        return pendingBookings.Select(b => new BookingDto
        {
            Id = b.Id,
            GuestId = b.GuestId,
            RoomId = b.RoomId,
            CheckIn = b.CheckIn,
            CheckOut = b.CheckOut,
            Occupants = b.Occupants,
            Status = b.Status,
            GroupReference = b.GroupReference
        }).ToList();
    }
}