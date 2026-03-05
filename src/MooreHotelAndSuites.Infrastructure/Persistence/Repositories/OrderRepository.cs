using Microsoft.EntityFrameworkCore;
using MooreHotelAndSuites.Domain.Entities;
using MooreHotelAndSuites.Domain.Enums;
using MooreHotelAndSuites.Application.Interfaces.Repositories;
using MooreHotelAndSuites.Infrastructure.Data;

namespace MooreHotelAndSuites.Infrastructure.Persistence.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ServiceOrder order)
        {
            await _context.Set<ServiceOrder>().AddAsync(order);
             await _context.SaveChangesAsync();
        }
          public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<List<ServiceOrder>> GetBySourceAsync(
            OrderSource source)
        {
            return await _context.Set<ServiceOrder>()
                .Include(x => x.Items)
                .Where(x => x.Source == source)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
      
        public async Task<List<ServiceOrder>> GetConfirmedBySourcesAsync(
            params OrderSource[] sources)
        {
            return await _context.Set<ServiceOrder>()
                .Include(x => x.Items)
                .Where(x =>
                    sources.Contains(x.Source) &&
                    x.Status == OrderStatus.Confirmed)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
        
public async Task<ServiceOrder?> GetPendingByCustomerAsync(string name, string phone, decimal amount)
{
    
    var order = await _context.ServiceOrders
        .Where(s =>
            s.CustomerName.Trim().ToLower() == name.Trim().ToLower() &&
            s.PhoneNumber.Trim() == phone.Trim() &&
            s.Status == OrderStatus.PendingPayment)
        .FirstOrDefaultAsync();

    if (order == null)
    {
        Console.WriteLine($"Order not found for: {name}, {phone}");
        return null;
    }

   
    if (Math.Abs(order.TotalAmount - amount) > 0.01m)
    {
        Console.WriteLine($"Amount mismatch: DB={order.TotalAmount}, Request={amount}");
        return null;
    }

    return order;
}
public async Task<ServiceOrder?> GetActiveForServingAsync(
    string name,
    string phone)
{
    return await _context.ServiceOrders
        .Include(x => x.Items)
        .Where(x =>
            x.CustomerName == name &&
            x.PhoneNumber == phone &&
            x.Status == OrderStatus.Confirmed)
        .OrderBy(x => x.CreatedAt)   // serve oldest first
        .FirstOrDefaultAsync();
}

        public async Task<List<ServiceOrder>> GetPendingBySourceAsync(
            OrderSource source)
        {
            return await _context.Set<ServiceOrder>()
                .Include(x => x.Items)
                .Where(x =>
                    x.Source == source &&
                    x.Status == OrderStatus.PendingPayment)
                .ToListAsync();
        }
       public async Task<List<ServiceOrder>> GetAllAsync()
    {
        return await _context.ServiceOrders
            .Include(x => x.Items)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }
    public async Task<List<ServiceOrder>> GetAllPendingByGuestAsync(string name, string phone)
{
    return await _context.ServiceOrders
        .Where(s =>
            s.CustomerName.Trim().ToLower() == name.Trim().ToLower() &&
            s.PhoneNumber.Trim() == phone.Trim() &&
            s.Status == OrderStatus.PendingPayment)
        .OrderByDescending(s => s.CreatedAt)
        .ToListAsync();
}
public async Task<List<ServiceOrder>> GetAllOrdersAsync()
{
    return await _context.ServiceOrders
        .Include(o => o.Guest)  // ✅ Include guest for name/phone
        .OrderByDescending(o => o.CreatedAt)
        .ToListAsync();
}

public async Task<List<ServiceOrder>> GetAllPendingOrdersAsync()
{
    return await _context.ServiceOrders
        .Include(o => o.Guest)
        .Where(o => o.Status == OrderStatus.PendingPayment)
        .OrderByDescending(o => o.CreatedAt)
        .ToListAsync();
}

public async Task<List<ServiceOrder>> GetAllConfirmedOrdersAsync()
{
    return await _context.ServiceOrders
        .Include(o => o.Guest)
        .Where(o => o.Status == OrderStatus.Confirmed)
        .OrderByDescending(o => o.CreatedAt)
        .ToListAsync();
}
        public async Task<ServiceOrder?> GetByIdAsync(Guid id)
        {
            return await _context.Set<ServiceOrder>()
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
