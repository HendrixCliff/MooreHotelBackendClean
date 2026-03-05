using MooreHotelAndSuites.Application.DTOs.Rooms;

namespace MooreHotelAndSuites.Application.Interfaces.Services
{
    public interface IRoomCommandService
    {
        Task<Guid> CreateAsync(CreateRoomDto dto);

        Task AddImageAsync(Guid roomId, CreateRoomImageDto image);
        Task UpdateAsync(Guid id, UpdateRoomDto dto);

        Task DeleteAsync(Guid roomId);

        Task UpdateImagesAsync(
            Guid roomId,
            IReadOnlyCollection<CreateRoomImageDto> images);

        Task UpdateStatusAsync(UpdateRoomStatusDto dto);

        Task UpdateRatingAsync(Guid roomId);
       Task DeleteImageAsync(Guid imageId);
}
}