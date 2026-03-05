using MooreHotelAndSuites.Application.DTOs.Guests;
using MooreHotelAndSuites.Domain.Entities;

namespace MooreHotelAndSuites.Application.Interfaces.Services
{
  public interface IGuestService
{
    

    Task<Guest?> GetByIdAsync(int id);
    Task<int> EnsureGuestAsync(string fullName, string email, string phone);

     Task<Guest?> FindByNameAsync(string fullName);

    Task<Guest?> FindByPhoneAsync(string phone);
}

}
