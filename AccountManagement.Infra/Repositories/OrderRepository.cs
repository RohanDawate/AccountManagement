using AccountManagement.Application.Interfaces;
using AccountManagement.Domain.Entities;

namespace AccountManagement.Infra.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        // Static in-memory list
        private static readonly List<Order> _orders = new();

        public Task<IEnumerable<Order>> GetAllAsync()
        {
            return Task.FromResult(_orders.AsEnumerable());
        }

        public Task<Order?> GetByIdAsync(long id)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id);
            return Task.FromResult(order);
        }

        public Task AddAsync(Order order)
        {
            _orders.Add(order);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Order order)
        {
            var existing = _orders.FirstOrDefault(o => o.Id == order.Id);
            existing?.Update(order.CustomerName, order.TotalAmount);
            //if (existing != null)
            //{
            //    existing.Update(order.CustomerName, order.TotalAmount);
            //}
            return Task.CompletedTask;
        }

        public Task DeleteAsync(long id)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id);
            if (order != null)
            {
                _orders.Remove(order);
            }
            return Task.CompletedTask;
        }
    }
}
