using MediatR;
using MooreHotelAndSuites.Application.DTOs.Booking;

namespace MooreHotelAndSuites.Application.Features.Bookings.Commands.CreateBooking;

public sealed record CreateBookingCommand(
    CreateBookingRequestDto Dto
) : IRequest<List<BookingDto>>;  