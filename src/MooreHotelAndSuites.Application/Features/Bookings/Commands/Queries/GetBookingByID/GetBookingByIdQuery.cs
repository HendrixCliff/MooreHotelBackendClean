using MediatR;
using MooreHotelAndSuites.Application.DTOs.Booking;

namespace MooreHotelAndSuites.Application.Features.Bookings.Queries.GetBookingById;

public sealed record GetBookingByIdQuery(Guid BookingId)
    : IRequest<BookingDto?>;