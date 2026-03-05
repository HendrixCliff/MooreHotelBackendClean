using MooreHotelAndSuites.Domain.Entities;
using MooreHotelAndSuites.Domain.Enums;

namespace MooreHotelAndSuites.Application.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        Task AddAsync(ServiceOrder order);

        Task<List<ServiceOrder>> GetBySourceAsync(
            OrderSource source);

        Task<List<ServiceOrder>> GetConfirmedBySourcesAsync(
            params OrderSource[] sources);

        Task<ServiceOrder?> GetPendingByCustomerAsync(
            string name,
            string phone,
            decimal amount);

        Task<ServiceOrder?> GetActiveForServingAsync(
            string name,
            string phone);
        Task<List<ServiceOrder>> GetAllOrdersAsync(); 
        Task<List<ServiceOrder>> GetAllPendingOrdersAsync();
        Task<List<ServiceOrder>> GetAllConfirmedOrdersAsync();
        Task<List<ServiceOrder>> GetAllPendingByGuestAsync(string name, string phone);
        Task<List<ServiceOrder>> GetPendingBySourceAsync(
            OrderSource source);
        Task SaveChangesAsync();
        Task<ServiceOrder?> GetByIdAsync(Guid id);
        
        Task<List<ServiceOrder>> GetAllAsync();
    }
}
