using MooreHotelAndSuites.Domain.Entities;
using MooreHotelAndSuites.Application.Interfaces.Repositories;
using MooreHotelAndSuites.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MooreHotelAndSuites.Application.Common;

namespace MooreHotelAndSuites.Infrastructure.Persistence.Repositories
{
public class GuestRepository : IGuestRepository
{
    private readonly AppDbContext _db;
    public GuestRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(Guest guest)
    {
        await _db.Guests.AddAsync(guest);
        await _db.SaveChangesAsync();
    }

     public async Task<Guest?> GetByIdAsync(int id)
    {
        return await _db.Guests.FindAsync(id);
    }
    public async Task<List<Guest>> GetAllAsync()
    {
        return await _db.Guests
            .AsNoTracking()
            .ToListAsync();
    }
    public async Task<int> CountAsync()
{
    return await _db.Guests.CountAsync();
}
public async Task<Guest?> GetByFullNameAndPhoneAsync(string fullName, string phoneNumber)
        {
            return await _db.Guests
                .FirstOrDefaultAsync(g =>
                    g.FullName.Trim().ToLower() == fullName.Trim().ToLower() &&
                    g.PhoneNumber.Trim() == phoneNumber.Trim());
        }
 public async Task<Guest?> FindByNameAsync(string fullName)
    {
        return await _db.Guests
            .FirstOrDefaultAsync(g => g.FullName == fullName);
    }
    public async Task<Guest?> FindByEmailAsync(string email)
        => await _db.Guests
            .FirstOrDefaultAsync(g => g.Email.ToLower() == email.ToLower());

   public async Task<Guest?> FindByPhoneAsync(string phone)
    {
        return await _db.Guests
            .FirstOrDefaultAsync(g => g.PhoneNumber == phone);
    }
}

}
