using MediatR;
using MooreHotelAndSuites.Application.DTOs.Booking;
using MooreHotelAndSuites.Application.Interfaces.Repositories;
using MooreHotelAndSuites.Application.Interfaces.Events;
using MooreHotelAndSuites.Application.Interfaces.Services;
using MooreHotelAndSuites.Domain.Enums;

namespace MooreHotelAndSuites.Application.Features.Bookings.Commands.ConfirmPayment;

public sealed class ConfirmBookingPaymentCommandHandler
    : IRequestHandler<ConfirmBookingPaymentCommand, BookingDto>
{
    private readonly IBookingRepository _repo;
    private readonly IGuestService _guestService;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public ConfirmBookingPaymentCommandHandler(
        IBookingRepository repo,
        IGuestService guestService,
        IDomainEventDispatcher eventDispatcher)
    {
        _repo = repo;
        _guestService = guestService;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<BookingDto> Handle(
        ConfirmBookingPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var booking = await _repo.GetByIdAsync(request.BookingId);
        if (booking == null)
            throw new InvalidOperationException($"Booking with ID {request.BookingId} not found");

        if (booking.Status != BookingStatus.Pending)
            throw new InvalidOperationException($"Booking is not pending. Current status: {booking.Status}");

        var guest = await _guestService.GetByIdAsync(booking.GuestId);
        if (guest == null)
            throw new InvalidOperationException("Guest not found");

        var amount = booking.CalculateAmount();

        booking.AddPayment(
            amount,
            request.PaymentMethod,
            request.StaffId,
            guest.FullName
        );

        // FIX: Use UpdateAsync to properly update the existing record
        await _repo.UpdateAsync(booking);

        if (booking.DomainEvents.Any())
        {
            await _eventDispatcher.DispatchAsync(booking.DomainEvents);
            booking.ClearDomainEvents();
        }

        return new BookingDto
        {
            Id = booking.Id,
            GuestId = booking.GuestId,
            RoomId = booking.RoomId,
            CheckIn = booking.CheckIn,
            CheckOut = booking.CheckOut,
            Occupants = booking.Occupants,
            Status = booking.Status
        };
    }
}