
namespace MooreHotelAndSuites.Application.DTOs.Auth;

public sealed record AuthTokenResult(
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiry);
