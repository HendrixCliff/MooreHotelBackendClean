using MediatR;
using MooreHotelAndSuites.Application.Interfaces.Repositories;
using MooreHotelAndSuites.Application.Interfaces.Events;

namespace MooreHotelAndSuites.Application.Features.Bookings.Commands.CheckOut;

public sealed class CheckOutBookingCommandHandler
    : IRequestHandler<CheckOutBookingCommand, Unit>  // ✅ Must specify Unit
{
    private readonly IBookingRepository _repo;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public CheckOutBookingCommandHandler(
        IBookingRepository repo,
        IDomainEventDispatcher eventDispatcher)
    {
        _repo = repo;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<Unit> Handle(
        CheckOutBookingCommand request,
        CancellationToken cancellationToken)
    {
        var booking = await _repo.GetByIdAsync(request.BookingId);
        if (booking == null)
            throw new InvalidOperationException("Booking not found");

        booking.MarkAsCheckedOut();
        await _repo.UpdateAsync(booking);

        if (booking.DomainEvents.Any())
        {
            await _eventDispatcher.DispatchAsync(booking.DomainEvents);
            booking.ClearDomainEvents();
        }

        return Unit.Value; 
    }
}