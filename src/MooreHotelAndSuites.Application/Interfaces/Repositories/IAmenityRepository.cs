using MooreHotelAndSuites.Domain.Entities;

namespace MooreHotelAndSuites.Application.Interfaces.Repositories
{
    public interface IAmenityRepository
    {
        Task<Amenity> AddAsync(Amenity amenity);
        Task<Amenity?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<Amenity>> GetAllAsync();
        Task<Amenity?> UpdateAsync(Amenity amenity);
        Task<bool> DeleteAsync(Guid id);
    }
}
