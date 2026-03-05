using MooreHotelAndSuites.Domain.Entities;
using MooreHotelAndSuites.Application.DTOs.Amenities;

namespace MooreHotelAndSuites.Application.Interfaces.Services
{
    public interface IAmenityService
    {
        Task<Amenity> CreateAsync(CreateAmenityDto dto);
        Task<Amenity?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<Amenity>> GetAllAsync();
        Task<Amenity?> UpdateAsync(Guid id, UpdateAmenityDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
