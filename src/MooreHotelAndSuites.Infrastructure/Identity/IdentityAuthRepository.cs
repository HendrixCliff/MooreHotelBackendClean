
using Microsoft.AspNetCore.Identity;
using MooreHotelAndSuites.Application.DTOs.Auth;
using MooreHotelAndSuites.Application.Interfaces.Identity;
using Microsoft.EntityFrameworkCore;
namespace MooreHotelAndSuites.Infrastructure.Identity;

public sealed class IdentityAuthRepository : IIdentityAuthRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityAuthRepository(
        UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IApplicationUser?> FindByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<IApplicationUser?> FindByIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByIdAsync(userId);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<AuthOperationResult> ConfirmEmailAsync(
        string userId,
        string token,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return AuthOperationResult.Failure("User not found.");

        if (user.EmailConfirmed)
            return AuthOperationResult.Success(
                "Email already confirmed. You can now log in.");

        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (!result.Succeeded)
        {
            return AuthOperationResult.Failure(
                "Email confirmation failed. The token may have expired.",
                result.Errors.Select(e => e.Description));
        }

        return AuthOperationResult.Success(
            "Email confirmed successfully. You can now log in.");
    }

    public async Task<bool> CheckPasswordAsync(
        string userId,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user is not null &&
               await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<bool> IsLockedOutAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user is not null &&
               await _userManager.IsLockedOutAsync(user);
    }

    public async Task<IReadOnlyList<string>> GetRolesAsync(
    string userId,
    CancellationToken cancellationToken = default)
{
    var user = await _userManager.FindByIdAsync(userId)
        ?? throw new InvalidOperationException("User not found.");

    var roles = await _userManager.GetRolesAsync(user);

    return roles.ToArray();
}

    public async Task SaveRefreshTokenAsync(
        string userId,
        string refreshToken,
        DateTime expiry,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.SpecifyKind(
            expiry,
            DateTimeKind.Utc);

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }

   public async Task<IApplicationUser?> FindByRefreshTokenAsync(
    string refreshToken,
    CancellationToken cancellationToken = default)
{
    return await _userManager.Users
        .FirstOrDefaultAsync(
            u => u.RefreshToken == refreshToken &&
                 u.RefreshTokenExpiryTime > DateTime.UtcNow,
            cancellationToken);
}

    public async Task RevokeRefreshTokenAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return;

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = DateTime.MinValue;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }
}
