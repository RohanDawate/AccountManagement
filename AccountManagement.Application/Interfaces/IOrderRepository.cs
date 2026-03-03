using AccountManagement.Domain.Entities;

namespace AccountManagement.Application.Interfaces
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order?> GetByIdAsync(long id);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(long id);
    }
}
