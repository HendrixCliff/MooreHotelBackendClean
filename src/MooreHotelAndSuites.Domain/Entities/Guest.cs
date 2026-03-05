

namespace MooreHotelAndSuites.Domain.Entities
{
    public class Guest
{
    
    public int Id { get; private set; }

    public string FullName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string PhoneNumber { get; private set; } = string.Empty;
    
    public DateTime CreatedAt { get; private set; }
    private Guest() { }   // EF Core

    public Guest(string fullName, string email, string phone)
    {
        FullName = fullName;
        Email = email;
        PhoneNumber = phone;
        CreatedAt = DateTime.UtcNow;
    }

}


}
