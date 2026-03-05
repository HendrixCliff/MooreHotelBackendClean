using MooreHotelAndSuites.Domain.Entities;
using MooreHotelAndSuites.Domain.Enums;

namespace MooreHotelAndSuites.Application.Interfaces.Repositories
{
    public interface IMenuRepository
    {
        Task<MenuItem?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<MenuItem>> GetBySourceAsync(OrderSource source);
        Task<IReadOnlyList<MenuItem>> GetAllAsync();
        Task AddAsync(MenuItem menuItem);
        Task UpdateAsync(MenuItem menuItem);
        Task DeleteAsync(MenuItem menuItem);
    }
}