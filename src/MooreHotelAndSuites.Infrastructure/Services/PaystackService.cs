using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using MooreHotelAndSuites.Application.DTOs.Payments;
using MooreHotelAndSuites.Application.Interfaces.Services;

namespace MooreHotelAndSuites.Infrastructure.Services
{
    public class PaystackService : IPaystackService
    {
        private readonly PaystackSettings _settings;
        private readonly HttpClient _httpClient;

        public PaystackService(IOptions<PaystackSettings> settings)
        {
            _settings = settings.Value;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.SecretKey}");
        }

        public async Task<PaystackInitializeResponse> InitializePaymentAsync(
            string email, 
            decimal amount, 
            string bookingId)
        {
            var request = new
            {
                email = email,
                amount = (int)(amount * 100), // Paystack uses kobo/kwacha
                callback_url = $"{_settings.BaseUrl}/payment-complete",
                metadata = new
                {
                    booking_id = bookingId,
                    custom_fields = new[]
                    {
                        new { display_name = "Booking ID", variable_name = "booking_id", value = bookingId }
                    }
                }
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{_settings.BaseUrl}/transaction/initialize", 
                request);

            return await response.Content.ReadFromJsonAsync<PaystackInitializeResponse>();
        }

        public async Task<PaystackVerifyResponse> VerifyPaymentAsync(string reference)
        {
            var response = await _httpClient.GetAsync(
                $"{_settings.BaseUrl}/transaction/verify/{reference}");

            return await response.Content.ReadFromJsonAsync<PaystackVerifyResponse>();
        }
    }
}