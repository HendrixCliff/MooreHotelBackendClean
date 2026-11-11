
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MooreHotelAndSuites.Application.DTOs.Admin;
using MooreHotelAndSuites.Application.Interfaces.Identity;
using MooreHotelAndSuites.Domain.Constants;

namespace MooreHotelAndSuites.Infrastructure.Identity;

public sealed class UserManagementService : IUserManagementService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly RoleManager<IdentityRole> _roles;

    public UserManagementService(
        UserManager<ApplicationUser> users,
        RoleManager<IdentityRole> roles)
    {
        _users = users;
        _roles = roles;
    }

    public Task<int> CountUsersAsync()
    {
        return _users.Users.CountAsync();
    }

    public async Task<IReadOnlyList<IApplicationUser>> GetUsersInRoleAsync(
        string role)
    {
        var users = await _users.GetUsersInRoleAsync(role);

        return users
            .Select(user => (IApplicationUser)new ApplicationUserView(user))
            .ToList();
    }

    public async Task<IReadOnlyList<EmployeeDto>> GetEmployeesAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await _users.Users
            .AsNoTracking()
            .OrderBy(user => user.FullName)
            .ToListAsync(cancellationToken);

        var employees = new List<EmployeeDto>(users.Count);

        foreach (var user in users)
        {
            var roles = await _users.GetRolesAsync(user);

            employees.Add(new EmployeeDto
            {
                Id = user.Id,
                FullName = user.FullName,
                UserName = user.UserName,
                Email = user.Email,
                Role = roles.FirstOrDefault() ?? string.Empty,
                EmailConfirmed = user.EmailConfirmed,
                IsActive = user.IsActive
            });
        }

        return employees;
    }

    public async Task<IApplicationUser> CreateUserAsync(
        string email,
        string fullName,
        string password,
        string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("A role is required.", nameof(role));

        if (string.Equals(role, Roles.Admin, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Admin accounts cannot be created through employee onboarding.");
        }

        if (!await _roles.RoleExistsAsync(role))
        {
            throw new InvalidOperationException(
                $"The role '{role}' does not exist.");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            EmailConfirmed = true,
            MustChangePassword = false,
            CreatedOn = DateTime.UtcNow,
            IsActive = true
        };

        var createResult = await _users.CreateAsync(user, password);

        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ",
                    createResult.Errors.Select(error => error.Description)));
        }

        var roleResult = await _users.AddToRoleAsync(user, role);

        if (!roleResult.Succeeded)
        {
            await _users.DeleteAsync(user);

            throw new InvalidOperationException(
                string.Join("; ",
                    roleResult.Errors.Select(error => error.Description)));
        }

        return new ApplicationUserView(user);
    }

    public async Task ActivateAsync(string userId)
    {
        var user = await _users.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException(
                $"User '{userId}' was not found.");

        user.IsActive = true;

        var result = await _users.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ",
                    result.Errors.Select(error => error.Description)));
        }
    }

    public async Task DeactivateAsync(string userId)
    {
        var user = await _users.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException(
                $"User '{userId}' was not found.");

        user.IsActive = false;

        // Also revoke any existing refresh token.
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = DateTime.MinValue;

        var result = await _users.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ",
                    result.Errors.Select(error => error.Description)));
        }
    }

    public async Task DeleteAsync(string userId)
    {
        var user = await _users.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException(
                $"User '{userId}' was not found.");

        var result = await _users.DeleteAsync(user);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ",
                    result.Errors.Select(error => error.Description)));
        }
    }
}
