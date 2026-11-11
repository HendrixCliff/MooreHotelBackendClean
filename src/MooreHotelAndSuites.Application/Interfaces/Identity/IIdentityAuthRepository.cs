
using MooreHotelAndSuites.Application.DTOs.Auth;

namespace MooreHotelAndSuites.Application.Interfaces.Identity;

public interface IIdentityAuthRepository
{
    Task<IApplicationUser?> FindByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<IApplicationUser?> FindByIdAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<string> GenerateEmailConfirmationTokenAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<AuthOperationResult> ConfirmEmailAsync(
        string userId,
        string token,
        CancellationToken cancellationToken = default);

    Task<bool> CheckPasswordAsync(
        string userId,
        string password,
        CancellationToken cancellationToken = default);

    Task<bool> IsLockedOutAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetRolesAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task SaveRefreshTokenAsync(
        string userId,
        string refreshToken,
        DateTime expiry,
        CancellationToken cancellationToken = default);

    Task<IApplicationUser?> FindByRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task RevokeRefreshTokenAsync(
        string userId,
        CancellationToken cancellationToken = default);
}
