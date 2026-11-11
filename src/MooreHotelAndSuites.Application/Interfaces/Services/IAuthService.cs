
using MooreHotelAndSuites.Application.DTOs.Auth;
using MooreHotelAndSuites.Application.DTOs.Users;

namespace MooreHotelAndSuites.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthOperationResult> SignupAsync(
        CreateUserDto dto,
        string confirmationBaseUrl,
        CancellationToken cancellationToken = default);

    Task<AuthOperationResult> ConfirmEmailAsync(
        string userId,
        string token,
        CancellationToken cancellationToken = default);

    Task<AuthTokenResult?> LoginAsync(
        LoginDto dto,
        CancellationToken cancellationToken = default);

    Task<AuthTokenResult?> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);
}
