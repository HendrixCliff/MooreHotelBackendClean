using MooreHotelAndSuites.Application.Interfaces.Repositories;
using MooreHotelAndSuites.Application.Interfaces.Services;
using MooreHotelAndSuites.Domain.Entities;
using MooreHotelAndSuites.Domain.Events;
using MooreHotelAndSuites.Domain.Abstractions;


namespace MooreHotelAndSuites.Application.EventHandlers
{
    public sealed class PaymentConfirmedEventHandler
        : IDomainEventHandler<PaymentConfirmedDomainEvent>
    {
        private readonly IAuditLogRepository _auditRepo;
        private readonly IBookingRepository _bookingRepo;
        private readonly IEmailService _emailService;
        private readonly IGuestService _guestService;

        public PaymentConfirmedEventHandler(
            IAuditLogRepository auditRepo,
            IBookingRepository bookingRepo,
            IEmailService emailService,
            IGuestService guestService)
        {
            _auditRepo = auditRepo;
            _bookingRepo = bookingRepo;
            _emailService = emailService;
            _guestService = guestService;
        }

        public async Task HandleAsync(PaymentConfirmedDomainEvent notification)
        {
            var booking = await _bookingRepo.GetByIdAsync(notification.BookingId);
            if (booking == null) return;

            var guest = await _guestService.GetByIdAsync(booking.GuestId);
            if (guest == null || string.IsNullOrEmpty(guest.Email)) return;

        
            var totalAmount = booking.Payments.Sum(p => p.Amount);

            try
            {
                await _emailService.SendAsync(
                    to: guest.Email,
                    subject: $"Payment Receipt - {booking.Reference}",
                    body: BuildReceiptEmail(booking, guest, totalAmount)
                );
            }
            catch
            {
                await _auditRepo.AddAsync(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = booking.GuestId.ToString(),
                    Entity = nameof(Booking),
                    Action = "EMAIL_FAILED",
                    Method = "DomainEvent",
                    Path = $"booking/{booking.Id}",
                    StatusCode = 500,
                    OccurredAt = DateTime.UtcNow
                });
            }
        }

        private static string BuildReceiptEmail(Booking booking, Guest guest, decimal amount) =>
            $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Payment Confirmed - Moore Hotel & Suites</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
            min-height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 20px;
        }}
        .container {{
            max-width: 500px;
            width: 100%;
            background: white;
            border-radius: 20px;
            box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
            overflow: hidden;
        }}
        .header {{
            background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
            padding: 40px 30px;
            text-align: center;
        }}
        .checkmark {{
            font-size: 60px;
            margin-bottom: 15px;
        }}
        .logo {{
            font-size: 26px;
            font-weight: 700;
            color: white;
            letter-spacing: 1px;
        }}
        .logo span {{
            color: #1a1a2e;
        }}
        .content {{
            padding: 40px 30px;
        }}
        .title {{
            font-size: 24px;
            font-weight: 700;
            color: #1a1a2e;
            text-align: center;
            margin-bottom: 25px;
        }}
        .details {{
            background: #f7fafc;
            border-radius: 12px;
            padding: 25px;
            margin-bottom: 25px;
        }}
        .detail-row {{
            display: flex;
            justify-content: space-between;
            padding: 12px 0;
            border-bottom: 1px solid #e2e8f0;
        }}
        .detail-row:last-child {{
            border-bottom: none;
        }}
        .detail-label {{
            color: #718096;
            font-size: 14px;
        }}
        .detail-value {{
            color: #1a1a2e;
            font-weight: 600;
            font-size: 14px;
        }}
        .amount {{
            background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
            color: white;
            font-size: 28px;
            font-weight: 700;
            text-align: center;
            padding: 20px;
            border-radius: 12px;
            margin-bottom: 25px;
        }}
        .message {{
            color: #4a5568;
            line-height: 1.8;
            text-align: center;
            font-size: 15px;
        }}
        .footer {{
            background: #1a1a2e;
            padding: 25px 30px;
            text-align: center;
        }}
        .footer p {{
            color: rgba(255,255,255,0.7);
            font-size: 13px;
            margin-bottom: 8px;
        }}
        .footer strong {{
            color: #38ef7d;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='checkmark'>✓</div>
            <div class='logo'>MOORE <span>HOTEL</span></div>
        </div>
        <div class='content'>
            <div class='title'>Payment Confirmed!</div>
            <div class='amount'>${amount:N2}</div>
            <div class='details'>
                <div class='detail-row'>
                    <span class='detail-label'>Guest</span>
                    <span class='detail-value'>{guest.FullName}</span>
                </div>
                <div class='detail-row'>
                    <span class='detail-label'>Booking Reference</span>
                    <span class='detail-value'>{booking.Reference}</span>
                </div>
                <div class='detail-row'>
                    <span class='detail-label'>Check-In</span>
                    <span class='detail-value'>{booking.CheckIn:dddd, dd MMM yyyy}</span>
                </div>
                <div class='detail-row'>
                    <span class='detail-label'>Check-Out</span>
                    <span class='detail-value'>{booking.CheckOut:dddd, dd MMM yyyy}</span>
                </div>
            </div>
            <div class='message'>
                Thank you for choosing <strong>Moore Hotel & Suites</strong>!<br>
                Your reservation is now confirmed and guaranteed.<br><br>
                We look forward to welcoming you!
            </div>
        </div>
        <div class='footer'>
            <p>Questions? Contact us at <strong>info@moorehotel.com</strong></p>
            <p>© 2024 Moore Hotel & Suites. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }
}