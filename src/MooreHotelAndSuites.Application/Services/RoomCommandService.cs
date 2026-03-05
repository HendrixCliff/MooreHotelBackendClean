using MooreHotelAndSuites.Application.DTOs.Rooms;
using MooreHotelAndSuites.Application.Interfaces.Repositories;
using MooreHotelAndSuites.Application.Interfaces.Services;
using MooreHotelAndSuites.Domain.Entities;
using MooreHotelAndSuites.Domain.Enums;

namespace MooreHotelAndSuites.Application.Services
{
    public class RoomCommandService : IRoomCommandService
    {
        private readonly IRoomRepository _repo;
        private readonly IImageStorageService _imageStorage;

        public RoomCommandService(
            IRoomRepository repo,
            IImageStorageService imageStorage)
        {
            _repo = repo;
            _imageStorage = imageStorage;
        }

        public async Task<Guid> CreateAsync(CreateRoomDto dto)
        {
            if (!Enum.IsDefined(typeof(RoomType), dto.RoomType))
                throw new InvalidOperationException("Invalid room tier selected");

            var room = new Room
            {
                Id = Guid.NewGuid(),
                RoomNumber = dto.RoomNumber,
                RoomName = dto.RoomName,
                Description = dto.Description,
                PricePerNight = dto.PricePerNight,
                Capacity = dto.Capacity,
                Size = dto.Size,
                RoomType = (RoomType)dto.RoomType,
                Status = RoomStatus.Available,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(room);

            if (dto.AmenityIds.Any())
                await _repo.UpdateRoomAmenitiesAsync(room.Id, dto.AmenityIds);

           
            await _repo.SaveChangesAsync();

            return room.Id;
        }

        public async Task UpdateAsync(Guid id, UpdateRoomDto dto)
        {
            var room = await _repo.GetByIdAsync(id);
            if (room == null)
                throw new Exception("Room not found");

            room.RoomNumber = dto.RoomNumber;
            room.RoomName = dto.RoomName;
            room.Description = dto.Description;
            room.PricePerNight = dto.PricePerNight;
            room.Capacity = dto.Capacity;
            room.Size = dto.Size;
            room.RoomType = (RoomType)dto.RoomType;
            room.UpdatedAt = DateTime.UtcNow;

            if (dto.AmenityIds.Any())
                await _repo.UpdateRoomAmenitiesAsync(id, dto.AmenityIds);

            
            await _repo.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid roomId)
        {
            var room = await _repo.GetByIdAsync(roomId);

            if (room == null)
                throw new Exception("Room not found");

            if (room.Status == RoomStatus.Reserved ||
                room.Status == RoomStatus.Occupied)
            {
                throw new InvalidOperationException("Cannot delete a reserved or occupied room");
            }

            await _repo.DeleteAsync(room);

            await _repo.SaveChangesAsync();
        }

        public async Task AddImageAsync(Guid roomId, CreateRoomImageDto image)
        {
            var room = await _repo.GetByIdAsync(roomId);
            if (room == null)
                throw new Exception("Room not found");

            if (image.IsCover)
                await _repo.ClearCoverImageAsync(roomId);

            var entity = new RoomImage
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                ImageUrl = image.ImageUrl,
                IsCover = image.IsCover,
                DisplayOrder = image.DisplayOrder
            };

            await _repo.AddRoomImageAsync(entity);

         
            await _repo.SaveChangesAsync();
        }

        public async Task UpdateImagesAsync(Guid roomId, IReadOnlyCollection<CreateRoomImageDto> images)
        {
            var roomImages = images
                .Select(i => new RoomImage
                {
                    Id = Guid.NewGuid(),
                    RoomId = roomId,
                    ImageUrl = i.ImageUrl,
                    IsCover = i.IsCover,
                    DisplayOrder = i.DisplayOrder
                })
                .ToList()
                .AsReadOnly();

            await _repo.UpdateRoomImagesAsync(roomId, roomImages);

           
            await _repo.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(UpdateRoomStatusDto dto)
        {
            if (!Enum.IsDefined(typeof(RoomStatus), dto.Status))
        throw new InvalidOperationException($"Invalid room status: {dto.Status}");

            await _repo.UpdateRoomStatusAsync(dto.RoomId, (RoomStatus)dto.Status);

            
            await _repo.SaveChangesAsync();
        }

        public async Task DeleteImageAsync(Guid imageId)
        {
            var image = await _repo.GetImageByIdAsync(imageId);
            if (image == null)
                throw new Exception("Image not found");

            await _imageStorage.DeleteAsync(image.ImageUrl);

            await _repo.DeleteRoomImageAsync(image);

            
            await _repo.SaveChangesAsync();
        }

        public async Task UpdateRatingAsync(Guid roomId)
        {
            await _repo.UpdateRoomRatingAsync(roomId);

           
            await _repo.SaveChangesAsync();
        }
    }
}
