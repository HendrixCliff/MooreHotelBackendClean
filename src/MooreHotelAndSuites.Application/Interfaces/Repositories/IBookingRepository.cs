using MooreHotelAndSuites.Domain.Entities;
using MooreHotelAndSuites.Domain.Enums;

namespace MooreHotelAndSuites.Application.Interfaces.Repositories
{
    public interface IBookingRepository
    {
       Task<Booking?> GetByIdAsync(Guid id);
        Task AddAsync(Booking booking);
        Task<IEnumerable<Booking>> GetByRoomAsync(Guid roomId);
        Task<Booking?> GetLastPendingAsync();
        Task<Booking?> GetActiveByUserAccountIdAsync(string userAccountId);
         Task<IEnumerable<Booking>> GetAllAsync(); 
        Task<IEnumerable<Booking>> GetAllPendingAsync();
        Task<Booking?> GetRecentPendingByGuestAsync(    int guestId, TimeSpan window);
      Task<Guest?> GetGuestByIdAsync(int guestId);  
       Task<Booking?> GetActiveByRoomNumberAsync(string roomNumber);
       Task<IEnumerable<Booking>> GetAllByStatusAsync(BookingStatus status);
       Task<Booking?> GetActiveByGuestAsync( string customerName, string phone);
        Task<Booking?> GetLastPendingByGuestIdAsync(int guestId);
        Task ReloadAsync(Booking booking); 
        Task UpdateAsync(Booking booking);
        Task SaveChangesAsync();
        Task DeleteAsync(Booking booking);
    }
}
