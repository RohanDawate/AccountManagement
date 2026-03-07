using AccountManagement.Application.DTOs;
using AccountManagement.Application.Exceptions;
using AccountManagement.Application.Interfaces;
using AccountManagement.Domain.Entities;

namespace AccountManagement.Application.Services
{

    public interface IOrderService
    {
        public Task<IEnumerable<Order>> GetAllAsync();

        public Task<Order> GetByIdAsync(long id);

        public Task<Order> CreateAsync(OrderDto dto);

        public Task<Order> UpdateAsync(long id, OrderDto dto);

        public Task DeleteAsync(long id);
    }

    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repository;

        public OrderService(IOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Order> GetByIdAsync(long id)
        {
            //return await _repository.GetByIdAsync(id);
            return await _repository.GetByIdAsync(id)
                    ?? throw new NotFoundException($"Order with id {id} not found.");
        }

        public async Task<Order> CreateAsync(OrderDto dto)
        {
            var order = new Order(dto.CustomerName, dto.TotalAmount);
            await _repository.AddAsync(order);

            return order;
        }

        public async Task<Order> UpdateAsync(long id, OrderDto dto)
        {
            var order = await _repository.GetByIdAsync(id)
                ?? throw new BusinessException("Order not found");

            order.Update(dto.CustomerName, dto.TotalAmount);
            await _repository.UpdateAsync(order);

            return order;
        }

        public async Task DeleteAsync(long id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
