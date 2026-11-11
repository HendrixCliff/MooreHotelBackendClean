namespace MooreHotelAndSuites.Application.DTOs.Admin;

public sealed class EmployeeDto
{
    public string Id { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public bool IsActive { get; set; }
}