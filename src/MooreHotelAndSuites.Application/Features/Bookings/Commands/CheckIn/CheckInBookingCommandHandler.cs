using MediatR;
using MooreHotelAndSuites.Application.Interfaces.Repositories;
using MooreHotelAndSuites.Application.Interfaces.Events;

namespace MooreHotelAndSuites.Application.Features.Bookings.Commands.CheckIn;

public sealed class CheckInBookingCommandHandler
    : IRequestHandler<CheckInBookingCommand, Unit>  
{
    private readonly IBookingRepository _repo;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public CheckInBookingCommandHandler(
        IBookingRepository repo,
        IDomainEventDispatcher eventDispatcher)
    {
        _repo = repo;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<Unit> Handle(
        CheckInBookingCommand request,
        CancellationToken cancellationToken)
    {
        var booking = await _repo.GetByIdAsync(request.BookingId);
        if (booking == null)
            throw new InvalidOperationException("Booking not found");

        booking.MarkAsCheckedIn();
        await _repo.UpdateAsync(booking);

        if (booking.DomainEvents.Any())
        {
            await _eventDispatcher.DispatchAsync(booking.DomainEvents);
            booking.ClearDomainEvents();
        }

        return Unit.Value; 
    }
}