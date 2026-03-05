using MediatR;

namespace MooreHotelAndSuites.Application.Features.Bookings.Commands.CheckOut;

public sealed record CheckOutBookingCommand(Guid BookingId)
    : IRequest<Unit>;