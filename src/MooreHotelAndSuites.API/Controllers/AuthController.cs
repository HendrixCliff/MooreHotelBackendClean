
using Microsoft.AspNetCore.Mvc;
using MooreHotelAndSuites.API.Extensions;
using MooreHotelAndSuites.Application.DTOs.Auth;
using MooreHotelAndSuites.Application.DTOs.Users;
using MooreHotelAndSuites.Application.Interfaces.Services;

namespace MooreHotelAndSuites.API.Controllers;

[ApiController]
[Route("api/users")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthController(
        IAuthService authService,
        IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup(
        [FromBody] CreateUserDto dto,
        CancellationToken cancellationToken)
    {
        var baseUrl = _configuration["BASE_URL"]
            ?? $"{Request.Scheme}://{Request.Host}";

        var result = await _authService.SignupAsync(
            dto,
            baseUrl,
            cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = result.Message,
                errors = result.Errors
            });
        }

        return Ok(new
        {
            message = result.Message,
            email = dto.Email,
            role = "User"
        });
    }

    [HttpGet("confirmemail")]
    public async Task<IActionResult> ConfirmEmail(
        [FromQuery] string userId,
        [FromQuery] string token,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new
            {
                message = "Invalid confirmation request."
            });
        }

        var result = await _authService.ConfirmEmailAsync(
            userId,
            token,
            cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = result.Message,
                errors = result.Errors
            });
        }

        return Ok(new
        {
            message = result.Message
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginDto dto,
        CancellationToken cancellationToken)
    {
        var tokens = await _authService.LoginAsync(
            dto,
            cancellationToken);

        if (tokens is null)
        {
            return Unauthorized(new
            {
                message = "Invalid credentials or email not confirmed."
            });
        }

        RefreshTokenCookie.Set(
            Response,
            tokens.RefreshToken,
            tokens.RefreshTokenExpiry);

        return Ok(new
        {
            accessToken = tokens.AccessToken
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[
            RefreshTokenCookie.CookieName];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized(new
            {
                message = "Refresh token is missing."
            });
        }

        var tokens = await _authService.RefreshAsync(
            refreshToken,
            cancellationToken);

        if (tokens is null)
        {
            RefreshTokenCookie.Clear(Response);

            return Unauthorized(new
            {
                message = "Invalid or expired refresh token."
            });
        }

        RefreshTokenCookie.Set(
            Response,
            tokens.RefreshToken,
            tokens.RefreshTokenExpiry);

        return Ok(new
        {
            accessToken = tokens.AccessToken
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[
            RefreshTokenCookie.CookieName];

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await _authService.LogoutAsync(
                refreshToken,
                cancellationToken);
        }

        RefreshTokenCookie.Clear(Response);

        return Ok(new
        {
            message = "Logged out successfully."
        });
    }
}
