namespace MooreHotelAndSuites.Application.DTOs.Admin
{
public class OnboardStaffDto
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = "Receptionist";
}

}
