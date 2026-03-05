namespace MooreHotelAndSuites.Application.Interfaces.Services
{
    public interface IPaystackService
    {
        Task<PaystackInitializeResponse> InitializePaymentAsync(string email, decimal amount, string bookingId);
        Task<PaystackVerifyResponse> VerifyPaymentAsync(string reference);
    }

    public class PaystackInitializeResponse
    {
        public bool Status { get; set; }
        public string? Message { get; set; }
        public PaystackData? Data { get; set; }
    }

    public class PaystackData
    {
        public string? AuthorizationUrl { get; set; }
        public string? AccessCode { get; set; }
        public string? Reference { get; set; }
    }

    public class PaystackVerifyResponse
    {
        public bool Status { get; set; }
        public string? Message { get; set; }
        public PaystackPaymentData? Data { get; set; }
    }

    public class PaystackPaymentData
    {
        public string? Reference { get; set; }
        public string? Status { get; set; }
        public decimal Amount { get; set; }
        public string? CustomerEmail { get; set; }
        public string? Metadata { get; set; }
    }
}