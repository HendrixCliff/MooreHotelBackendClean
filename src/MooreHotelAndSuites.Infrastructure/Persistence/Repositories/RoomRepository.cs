using Microsoft.EntityFrameworkCore;
using MooreHotelAndSuites.Domain.Entities;
using MooreHotelAndSuites.Domain.Enums;
using MooreHotelAndSuites.Application.Interfaces.Repositories;
using MooreHotelAndSuites.Infrastructure.Data;

namespace MooreHotelAndSuites.Infrastructure.Persistence.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly AppDbContext _db;

        public RoomRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Room?> GetByIdAsync(Guid roomId)
        {
            return await _db.Rooms
                .Include(r => r.Images)
                .Include(r => r.RoomAmenities)
                    .ThenInclude(ra => ra.Amenity)
                .Include(r => r.Reviews)
                .FirstOrDefaultAsync(r => r.Id == roomId);
        }

        public async Task<IReadOnlyList<Room>> GetAllAsync()
        {
            return await _db.Rooms
                .Include(r => r.Images)
                .Include(r => r.RoomAmenities)
                    .ThenInclude(ra => ra.Amenity)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Room>> GetAvailableRoomsAsync(
            DateTime checkIn,
            DateTime checkOut,
            int guests)
        {
            return await _db.Rooms
                .Include(r => r.Images)
                .Include(r => r.Reservations)
                .Where(r =>
                    r.Capacity >= guests &&
                    r.Status != RoomStatus.Maintenance &&
                    r.Status != RoomStatus.OutOfService &&
                    !r.Reservations.Any(res =>
                        res.Status != ReservationStatus.Cancelled &&
                        res.CheckIn < checkOut &&
                        res.CheckOut > checkIn))
                .ToListAsync();
        }

        public Task AddAsync(Room room)
        {
            _db.Rooms.Add(room);
            return Task.CompletedTask;
        }
        public Task UpdateAsync(Room room)
        {
            _db.Rooms.Update(room);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Room room)
        {
            _db.Rooms.Remove(room);
            return Task.CompletedTask;
        }

        public Task AddRoomImageAsync(RoomImage image)
        {
            _db.RoomImages.Add(image);
            return Task.CompletedTask;
        }

            public Task ClearCoverImageAsync(Guid roomId)
        {
            var covers = _db.RoomImages
                .Where(x => x.RoomId == roomId && x.IsCover);

            foreach (var img in covers)
                img.IsCover = false;

            return Task.CompletedTask;
        }

        public async Task UpdateRoomAmenitiesAsync(Guid roomId, IReadOnlyCollection<Guid> amenityIds)
        {
            var room = await _db.Rooms
                .Include(r => r.RoomAmenities)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (room == null) return;

            var validAmenities = await _db.Amenities
                .Where(a => amenityIds.Contains(a.Id))
                .Select(a => a.Id)
                .ToListAsync();

            room.RoomAmenities.Clear();

            foreach (var amenityId in validAmenities)
            {
                room.RoomAmenities.Add(new RoomAmenity
                {
                    RoomId = room.Id,
                    AmenityId = amenityId
                });
            }

            room.UpdatedAt = DateTime.UtcNow;
            
        }

        public async Task UpdateRoomImagesAsync(Guid roomId, IReadOnlyCollection<RoomImage> images)
        {
            var room = await _db.Rooms
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (room == null) return;

            room.Images.Clear();

            foreach (var image in images)
            {
                image.RoomId = roomId;
                room.Images.Add(image);
            }

            room.UpdatedAt = DateTime.UtcNow;
            
        }

        public async Task<RoomImage?> GetImageByIdAsync(Guid imageId)
        {
            return await _db.RoomImages.FirstOrDefaultAsync(x => x.Id == imageId);
        }

        public Task DeleteRoomImageAsync(RoomImage image)
        {
            _db.RoomImages.Remove(image);
            return Task.CompletedTask;
        }

        public async Task UpdateRoomStatusAsync(Guid roomId, RoomStatus status)
        {
            var room = await _db.Rooms.FindAsync(roomId);
            if (room == null) return;

            room.Status = status;
            room.UpdatedAt = DateTime.UtcNow;
          
        }

        public async Task UpdateRoomRatingAsync(Guid roomId)
        {
            var room = await _db.Rooms
                .Include(r => r.Reviews)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (room == null || !room.Reviews.Any())
                return;

            room.TotalReviews = room.Reviews.Count;
            room.AverageRating = room.Reviews.Average(r => r.Rating);
            room.UpdatedAt = DateTime.UtcNow;
            
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
