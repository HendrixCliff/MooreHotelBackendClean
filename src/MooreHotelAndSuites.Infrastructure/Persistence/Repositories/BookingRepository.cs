using MooreHotelAndSuites.Domain.Entities;
using MooreHotelAndSuites.Application.Interfaces.Repositories;
using MooreHotelAndSuites.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MooreHotelAndSuites.Domain.Enums;

namespace MooreHotelAndSuites.Infrastructure.Persistence.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _db;
        public BookingRepository(AppDbContext db) => _db = db;

        public async Task AddAsync(Booking booking) 
        { 
            await _db.Bookings.AddAsync(booking); 
            await _db.SaveChangesAsync(); 
        }

        public async Task<IEnumerable<Booking>> GetByRoomAsync(Guid roomId) 
            => await _db.Bookings.Where(b => b.RoomId == roomId).ToListAsync();

       public async Task<Booking?> GetByIdAsync(Guid id)
        {
            // Use AsNoTracking for read operations, then attach for writes
            return await _db.Bookings
                .Include(b => b.Room)
                .Include(b => b.Payments)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);
        }
            public async Task<Guest?> GetGuestByIdAsync(int guestId)
    {
        return await _db.Guests.FindAsync(guestId);
    }

        public async Task<Booking?> GetActiveByUserAccountIdAsync(string userAccountId)
        {
            return await _db.Bookings
                .Include(b => b.Guest)
                .Include(b => b.Room)
                .Where(b =>
                    b.UserAccountId == userAccountId &&
                    (b.Status == BookingStatus.CheckedIn ||
                    b.Status == BookingStatus.Reserved))
                .FirstOrDefaultAsync();
        }

        public async Task<Booking?> GetLastPendingAsync()
        {
            return await _db.Bookings
                .Where(b => b.Status == BookingStatus.Pending)
                .OrderBy(b => b.CheckIn)
                .FirstOrDefaultAsync();
        }

        public async Task<Booking?> GetRecentPendingByGuestAsync(
            int guestId,
            TimeSpan window)
        {
            var cutoff = DateTime.UtcNow.Subtract(window);

            return await _db.Bookings
                .Where(b =>
                    b.GuestId == guestId &&
                    b.Status == BookingStatus.Pending &&
                    b.CreatedAt >= cutoff)
                .FirstOrDefaultAsync();
        }

     public async Task<Booking?> GetActiveByGuestAsync(string name, string phone)
{
    return await _db.Bookings
        .Include(b => b.Room)
        .Where(b =>
            (b.Status == BookingStatus.CheckedIn ||
             b.Status == BookingStatus.Reserved)
            && _db.Guests.Any(g =>
                g.Id == b.GuestId &&
                g.FullName == name &&
                g.PhoneNumber == phone))
        .FirstOrDefaultAsync();
}
public async Task<Booking?> GetActiveByRoomNumberAsync(string roomNumber)
{
    return await _db.Bookings
        .Include(b => b.Room)
        .Include(b => b.Guest)
        .Where(b =>
            (b.Status == BookingStatus.CheckedIn ||
             b.Status == BookingStatus.Reserved)
            && b.Room.RoomNumber == roomNumber)
        .FirstOrDefaultAsync();
}
          public async Task ReloadAsync(Booking booking)
        {
            await _db.Entry(booking).ReloadAsync();
        }
        public async Task<Booking?> GetLastPendingByGuestIdAsync(int guestId)
        {
            return await _db.Bookings
                .Where(b =>
                    b.Status == BookingStatus.Pending &&
                    b.GuestId == guestId)
                .OrderByDescending(b => b.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Booking>> GetAllByStatusAsync(BookingStatus status)
        {
            return await _db.Bookings
                .Where(b => b.Status == status)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

  public async Task UpdateAsync(Booking booking)
        {
            // Get the existing tracked entity from DB
            var existingBooking = await _db.Bookings.FindAsync(booking.Id);

            if (existingBooking != null)
            {
                // Copy all modified values to the tracked entity
                _db.Entry(existingBooking).CurrentValues.SetValues(booking);
            }
            else
            {
                // If not found, attach as modified
                _db.Bookings.Update(booking);
            }

            await _db.SaveChangesAsync();
        }
        public async Task<IEnumerable<Booking>> GetAllAsync()
        {
            return await _db.Bookings
                .Include(b => b.Room)
                .Include(b => b.Guest)
                .Include(b => b.Payments)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetAllPendingAsync()
        {
            return await _db.Bookings
                .Where(b => b.Status == BookingStatus.Pending)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Booking booking)
        {
            _db.Bookings.Remove(booking);
            await _db.SaveChangesAsync();
        }
    }
}