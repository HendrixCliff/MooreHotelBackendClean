using Microsoft.AspNetCore.Mvc;
using MooreHotelAndSuites.Application.Interfaces.Repositories;
using MooreHotelAndSuites.Application.Interfaces.Services;
using MooreHotelAndSuites.Domain.Enums;
using System.Text.Json;

namespace MooreHotelAndSuites.API.Controllers
{
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaystackService _paystack;
        private readonly IBookingRepository _bookingRepo;
        private readonly IConfiguration _configuration;

        public PaymentsController(
            IPaystackService paystack,
            IBookingRepository bookingRepo,
            IConfiguration configuration)
        {
            _paystack = paystack;
            _bookingRepo = bookingRepo;
            _configuration = configuration;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            
            // Verify webhook signature
            var paystackSignature = Request.Headers["x-paystack-signature"].FirstOrDefault();
            var secret = _configuration["PaystackSettings:SecretKey"];
            
            // TODO: Verify signature for security
            // var computedSignature = ComputeSignature(json, secret);

            var payload = JsonSerializer.Deserialize<PaystackWebhookPayload>(json);

            if (payload?.Event == "charge.success")
            {
                var metadata = payload.Data?.Metadata;
                var bookingId = Guid.Parse(metadata?.BookingId ?? "");

                var booking = await _bookingRepo.GetByIdAsync(bookingId);
                if (booking != null && booking.Status == BookingStatus.Pending)
                {
                    var guest = await _bookingRepo.GetGuestByIdAsync(booking.GuestId);
                    
                    booking.AddPayment(
                        booking.CalculateAmount(),
                        "Paystack",
                        "System",
                        guest?.FullName ?? "Guest"
                    );

                    await _bookingRepo.UpdateAsync(booking);
                }
            }

            return Ok();
        }
    }

    public class PaystackWebhookPayload
    {
        public string? Event { get; set; }
        public WebhookData? Data { get; set; }
    }

    public class WebhookData
    {
        public string? Reference { get; set; }
        public WebhookMetadata? Metadata { get; set; }
    }

    public class WebhookMetadata
    {
        public string? BookingId { get; set; }
    }
}