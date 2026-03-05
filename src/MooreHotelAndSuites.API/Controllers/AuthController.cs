using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MooreHotelAndSuites.Infrastructure.Auth;
using MooreHotelAndSuites.Infrastructure.Identity;
using MooreHotelAndSuites.Application.Interfaces.Identity;
using Microsoft.EntityFrameworkCore;
using MooreHotelAndSuites.Application.Interfaces.Services;
using MooreHotelAndSuites.Application.DTOs.Auth;
using MooreHotelAndSuites.Application.DTOs.Users;
using MooreHotelAndSuites.Application.Services;
using System.Security.Claims;
using System.Net;




namespace MooreHotelAndSuites.API.Controllers
{
    [ApiController]
[Route("api/users")]
public class AuthController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwt;
     private readonly IUserManagementService _users;
     private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    public AuthController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,IUserManagementService users,
        IJwtTokenService jwt,  IEmailService emailService, IConfiguration configuration)
    {
        _signInManager = signInManager;
         _users = users;
        _userManager = userManager;
        _jwt = jwt;
        _emailService = emailService;
        _configuration = configuration;
    }
     

 [HttpPost("signup")]
public async Task<IActionResult> Create(CreateUserDto dto)
{
    var user = await _users.CreateUserAsync(
        dto.Email,
        dto.FullName,
        dto.Password,
        role: "User"
    );

    var identityUser = await _userManager.FindByEmailAsync(dto.Email);
    if (identityUser == null)
        throw new InvalidOperationException("User creation failed.");

    var token = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);
    var encodedToken = WebUtility.UrlEncode(token);

   
    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") 
                  ?? $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
    
    var link = $"{baseUrl}/api/auth/confirmemail?userId={identityUser.Id}&token={encodedToken}";

    await _emailService.SendAsync(
        dto.Email,
        "Confirm your account - Moore Hotel & Suites",
        EmailTemplates.ConfirmAccount(identityUser.FullName ?? identityUser.Email!, link)
    );

    return Ok(new
    {
        message = "User created. Confirmation email sent",
        email = dto.Email,
        role = "User"
    });
}

[HttpGet("confirmemail")]
public async Task<IActionResult> ConfirmEmail(string userId, string token)
{
    if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        return BadRequest(new { message = "Invalid confirmation request" });

    var user = await _userManager.FindByIdAsync(userId);
    if (user == null)
        return NotFound(new { message = "User not found" });

    if (user.EmailConfirmed)
        return Ok(new { message = "Email already confirmed. You can now log in." });

    var result = await _userManager.ConfirmEmailAsync(user, token);
    
    if (result.Succeeded)
    {
        return Ok(new 
        { 
            message = "Email confirmed successfully! You can now log in to your account.",
            email = user.Email
        });
    }

    return BadRequest(new 
    { 
        message = "Email confirmation failed. The token may have expired.",
        errors = result.Errors.Select(e => e.Description)
    });
}


[HttpPost("login")]
public async Task<IActionResult> Login(LoginDto dto)
{
    var user = await _userManager.FindByEmailAsync(dto.Email);

    if (user == null)
        return Unauthorized(new { message = "Invalid credentials - user not found" });

    if (!user.EmailConfirmed)
        return Unauthorized(new { message = "Email not confirmed" });

    var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
    if (!isPasswordValid)
        return Unauthorized(new { message = "Invalid credentials - wrong password" });

    if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
        return Unauthorized(new { message = "Account is locked" });

    try
    {
        var tokens = await _jwt.GenerateTokensAsync(user);
        
        user.RefreshToken = tokens.RefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _userManager.UpdateAsync(user);

        return Ok(new
        {
            accessToken = tokens.AccessToken,
            refreshToken = tokens.RefreshToken
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "Token generation failed", error = ex.Message });
    }
}

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(TokenRefreshDto model)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == model.RefreshToken);

        if (user == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            return Unauthorized("Invalid or expired refresh token");

        
        var newTokens = await _jwt.GenerateTokensAsync(user);

     
        user.RefreshToken = newTokens.RefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return Ok(new
        {
            accessToken = newTokens.AccessToken,
            refreshToken = newTokens.RefreshToken
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Ok();

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = DateTime.MinValue;
        await _userManager.UpdateAsync(user);

        return Ok("Logged out");
    }
}

    }


