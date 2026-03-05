using MediatR;
using MooreHotelAndSuites.Application.DTOs.Booking;

namespace MooreHotelAndSuites.Application.Features.Bookings.Queries.GetAllBookings;

public sealed record GetAllBookingsQuery()
    : IRequest<List<BookingDto>>;