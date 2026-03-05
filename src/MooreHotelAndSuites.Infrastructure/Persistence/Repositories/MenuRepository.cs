using Microsoft.EntityFrameworkCore;
using MooreHotelAndSuites.Application.Interfaces.Repositories;
using MooreHotelAndSuites.Domain.Entities;
using MooreHotelAndSuites.Domain.Enums;
using MooreHotelAndSuites.Infrastructure.Data;

namespace MooreHotelAndSuites.Infrastructure.Persistence.Repositories
{
    public class MenuRepository : IMenuRepository
    {
        private readonly AppDbContext _context;

        public MenuRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MenuItem?> GetByIdAsync(Guid id)
            => await _context.MenuItems.FindAsync(id);

        public async Task<IReadOnlyList<MenuItem>> GetBySourceAsync(OrderSource source)
            => await _context.MenuItems
                .Where(m => m.Source == source && m.IsAvailable)
                .ToListAsync();

        public async Task<IReadOnlyList<MenuItem>> GetAllAsync()
            => await _context.MenuItems.ToListAsync();

        public async Task AddAsync(MenuItem menuItem)
        {
            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(MenuItem menuItem)
        {
            _context.MenuItems.Update(menuItem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(MenuItem menuItem)
        {
            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();
        }
    }
}