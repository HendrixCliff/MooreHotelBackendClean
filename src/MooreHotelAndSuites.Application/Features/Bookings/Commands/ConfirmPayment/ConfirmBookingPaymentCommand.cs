using MediatR;
using MooreHotelAndSuites.Application.DTOs.Booking;

namespace MooreHotelAndSuites.Application.Features.Bookings.Commands.ConfirmPayment;

public sealed record ConfirmBookingPaymentCommand(
    Guid BookingId,
    string PaymentMethod,
    string StaffId
) : IRequest<BookingDto>;