
using System.Net;
using MooreHotelAndSuites.Application.DTOs.Auth;
using MooreHotelAndSuites.Application.DTOs.Users;
using MooreHotelAndSuites.Application.Interfaces.Identity;
using MooreHotelAndSuites.Application.Interfaces.Services;
using MooreHotelAndSuites.Application.Services;

namespace MooreHotelAndSuites.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserManagementService _users;
    private readonly IIdentityAuthRepository _identity;
    private readonly IJwtTokenService _jwt;
    private readonly IEmailService _emailService;

    public AuthService(
        IUserManagementService users,
        IIdentityAuthRepository identity,
        IJwtTokenService jwt,
        IEmailService emailService)
    {
        _users = users;
        _identity = identity;
        _jwt = jwt;
        _emailService = emailService;
    }

    public async Task<AuthOperationResult> SignupAsync(
        CreateUserDto dto,
        string confirmationBaseUrl,
        CancellationToken cancellationToken = default)
    {
        var existingUser = await _identity.FindByEmailAsync(
            dto.Email,
            cancellationToken);

        if (existingUser is not null)
        {
            return AuthOperationResult.Failure(
                "An account with this email already exists.");
        }

        var user = await _users.CreateUserAsync(
            dto.Email,
            dto.FullName,
            dto.Password,
            "User");

        var token = await _identity.GenerateEmailConfirmationTokenAsync(
            user.Id,
            cancellationToken);

        var encodedToken = WebUtility.UrlEncode(token);

        var link =
            $"{confirmationBaseUrl.TrimEnd('/')}/api/users/confirmemail" +
            $"?userId={Uri.EscapeDataString(user.Id)}&token={encodedToken}";

        var displayName = user.FullName ?? user.Email ?? dto.Email;

        await _emailService.SendAsync(
            dto.Email,
            "Confirm your account - Moore Hotel & Suites",
            EmailTemplates.ConfirmAccount(displayName, link));

        return AuthOperationResult.Success(
            "User created. Confirmation email sent.");
    }

    public Task<AuthOperationResult> ConfirmEmailAsync(
        string userId,
        string token,
        CancellationToken cancellationToken = default)
    {
        return _identity.ConfirmEmailAsync(
            userId,
            token,
            cancellationToken);
    }

    public async Task<AuthTokenResult?> LoginAsync(
        LoginDto dto,
        CancellationToken cancellationToken = default)
    {
        var user = await _identity.FindByEmailAsync(
            dto.Email,
            cancellationToken);

       if (user is null || !user.EmailConfirmed || !user.IsActive)
       return null;

        if (await _identity.IsLockedOutAsync(
                user.Id,
                cancellationToken))
        {
            return null;
        }

        var passwordValid = await _identity.CheckPasswordAsync(
            user.Id,
            dto.Password,
            cancellationToken);

        if (!passwordValid)
            return null;

        var roles = await _identity.GetRolesAsync(
            user.Id,
            cancellationToken);

        var accessToken = await _jwt.GenerateAccessTokenAsync(
            user,
            roles,
            cancellationToken);

        var refreshToken = _jwt.GenerateRefreshToken();
        var refreshExpiry = DateTime.UtcNow.AddDays(7);

        await _identity.SaveRefreshTokenAsync(
            user.Id,
            refreshToken,
            refreshExpiry,
            cancellationToken);

        return new AuthTokenResult(
            accessToken,
            refreshToken,
            refreshExpiry);
    }

    public async Task<AuthTokenResult?> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var user = await _identity.FindByRefreshTokenAsync(
            refreshToken,
            cancellationToken);

        if (user is null || !user.EmailConfirmed || !user.IsActive)
        return null;

        var roles = await _identity.GetRolesAsync(
            user.Id,
            cancellationToken);

        var accessToken = await _jwt.GenerateAccessTokenAsync(
            user,
            roles,
            cancellationToken);

        var newRefreshToken = _jwt.GenerateRefreshToken();
        var refreshExpiry = DateTime.UtcNow.AddDays(7);

        // Rotate the refresh token. The old token becomes invalid.
        await _identity.SaveRefreshTokenAsync(
            user.Id,
            newRefreshToken,
            refreshExpiry,
            cancellationToken);

        return new AuthTokenResult(
            accessToken,
            newRefreshToken,
            refreshExpiry);
    }

    public async Task LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var user = await _identity.FindByRefreshTokenAsync(
            refreshToken,
            cancellationToken);

        if (user is not null)
        {
            await _identity.RevokeRefreshTokenAsync(
                user.Id,
                cancellationToken);
        }
    }
}
