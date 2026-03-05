using MediatR;
using MooreHotelAndSuites.Application.DTOs.Payments;

namespace MooreHotelAndSuites.Application.Features.Bookings.Commands.ProcessPayment;

public record ProcessPaymentCommand(Guid BookingId, string ReturnUrl) 
    : IRequest<PaymentLinkDto>;

public class PaymentLinkDto
{
    public string AuthorizationUrl { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
}