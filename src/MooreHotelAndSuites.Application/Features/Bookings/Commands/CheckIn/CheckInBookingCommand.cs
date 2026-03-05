using MediatR;

namespace MooreHotelAndSuites.Application.Features.Bookings.Commands.CheckIn;

public sealed record CheckInBookingCommand(Guid BookingId)
    : IRequest<Unit>;