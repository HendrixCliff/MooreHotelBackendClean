using MediatR;
using MooreHotelAndSuites.Application.DTOs.Payments;
using MooreHotelAndSuites.Application.Interfaces.Repositories;
using MooreHotelAndSuites.Application.Interfaces.Services;
using MooreHotelAndSuites.Domain.Enums;

namespace MooreHotelAndSuites.Application.Features.Bookings.Commands.ProcessPayment;

public sealed class ProcessPaymentCommandHandler
    : IRequestHandler<ProcessPaymentCommand, PaymentLinkDto>
{
    private readonly IBookingRepository _repo;
    private readonly IPaystackService _paystack;
    private readonly IGuestService _guestService;

    public ProcessPaymentCommandHandler(
        IBookingRepository repo,
        IPaystackService paystack,
        IGuestService guestService)
    {
        _repo = repo;
        _paystack = paystack;
        _guestService = guestService;
    }

    public async Task<PaymentLinkDto> Handle(
        ProcessPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var booking = await _repo.GetByIdAsync(request.BookingId);
        if (booking == null)
            throw new InvalidOperationException("Booking not found");

        if (booking.Status != BookingStatus.Pending)
            throw new InvalidOperationException("Only pending bookings can be paid");

        var guest = await _guestService.GetByIdAsync(booking.GuestId);
        if (guest == null || string.IsNullOrEmpty(guest.Email))
            throw new InvalidOperationException("Guest email not found");

        var amount = booking.CalculateAmount();

        // Initialize Paystack payment
        var response = await _paystack.InitializePaymentAsync(
            guest.Email,
            amount,
            booking.Id.ToString()
        );

        if (!response.Status || response.Data == null)
            throw new InvalidOperationException($"Payment initialization failed: {response.Message}");

        // Store reference for verification later
        // booking.SetPaymentReference(response.Data.Reference);
        // await _repo.UpdateAsync(booking);

       if (string.IsNullOrWhiteSpace(response.Data.AuthorizationUrl))
        {
            throw new InvalidOperationException(
                "Paystack returned no authorization URL");
        }

        if (string.IsNullOrWhiteSpace(response.Data.Reference))
        {
            throw new InvalidOperationException(
                "Paystack returned no reference");
        }

return new PaymentLinkDto
{
    AuthorizationUrl = response.Data.AuthorizationUrl,
    Reference = response.Data.Reference
};
    }
}