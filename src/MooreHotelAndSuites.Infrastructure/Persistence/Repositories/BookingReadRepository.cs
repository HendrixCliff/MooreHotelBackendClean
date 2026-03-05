
using MooreHotelAndSuites.Application.Interfaces.Repositories;
using MooreHotelAndSuites.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;



namespace MooreHotelAndSuites.Infrastructure.Persistence.Repositories
{
public class BookingReadRepository : IBookingReadRepository
{
    private readonly AppDbContext _context;

    public BookingReadRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<int> CountActiveBookingsAsync()
    {
        var now = DateTime.UtcNow;

        return _context.Bookings.CountAsync(b =>
            b.CheckIn <= now &&
            b.CheckOut >= now);
    }

    public Task<int> CountTodayAsync()
    {
        var today = DateTime.UtcNow.Date;

        return _context.Bookings.CountAsync(b =>
            b.CheckIn.Date == today);
    }
    public Task<decimal> SumRevenueTodayAsync()
{
    var today = DateTime.UtcNow.Date;

    return _context.Payments
        .Where(p => p.CreatedAt.Date == today)
        .SumAsync(p => p.Amount);
}
}

}
