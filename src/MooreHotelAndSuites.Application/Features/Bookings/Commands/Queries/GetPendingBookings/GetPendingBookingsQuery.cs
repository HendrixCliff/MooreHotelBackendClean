using MediatR;
using MooreHotelAndSuites.Application.DTOs.Booking;

public sealed record GetPendingBookingsQuery() : IRequest<List<BookingDto>>;