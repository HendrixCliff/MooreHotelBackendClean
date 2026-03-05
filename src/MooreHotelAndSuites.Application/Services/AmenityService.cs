using MooreHotelAndSuites.Application.DTOs.Amenities;
using MooreHotelAndSuites.Application.Interfaces.Repositories;
using MooreHotelAndSuites.Application.Interfaces.Services;
using MooreHotelAndSuites.Domain.Entities;

namespace MooreHotelAndSuites.Application.Services
{
    public class AmenityService : IAmenityService
    {
        private readonly IAmenityRepository _repo;

        public AmenityService(IAmenityRepository repo)
        {
            _repo = repo;
        }

        public async Task<Amenity> CreateAsync(CreateAmenityDto dto)
        {
            var amenity = new Amenity { Id = Guid.NewGuid(), Name = dto.Name };
            return await _repo.AddAsync(amenity);
        }

        public async Task<Amenity?> GetByIdAsync(Guid id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<IReadOnlyList<Amenity>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<Amenity?> UpdateAsync(Guid id, UpdateAmenityDto dto)
        {
            var amenity = await _repo.GetByIdAsync(id);
            if (amenity == null) return null;

            amenity.Name = dto.Name;
            return await _repo.UpdateAsync(amenity);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _repo.DeleteAsync(id);
        }
    }
}
