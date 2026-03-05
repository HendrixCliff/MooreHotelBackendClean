using Microsoft.EntityFrameworkCore;
using MooreHotelAndSuites.Application.Interfaces.Repositories;
using MooreHotelAndSuites.Domain.Entities;
using MooreHotelAndSuites.Infrastructure.Data;

namespace MooreHotelAndSuites.Infrastructure.Persistence.Repositories
{
    public class AmenityRepository : IAmenityRepository
    {
        private readonly AppDbContext _db;

        public AmenityRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Amenity> AddAsync(Amenity amenity)
        {
            _db.Amenities.Add(amenity);
            await _db.SaveChangesAsync();
            return amenity;
        }

        public async Task<Amenity?> GetByIdAsync(Guid id)
        {
            return await _db.Amenities.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IReadOnlyList<Amenity>> GetAllAsync()
        {
            return await _db.Amenities.ToListAsync();
        }

        public async Task<Amenity?> UpdateAsync(Amenity amenity)
        {
            _db.Amenities.Update(amenity);
            await _db.SaveChangesAsync();
            return amenity;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var amenity = await _db.Amenities.FirstOrDefaultAsync(a => a.Id == id);
            if (amenity == null) return false;

            _db.Amenities.Remove(amenity);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
