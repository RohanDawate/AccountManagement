using AccountManagement.Application.Common.Responses;
using AccountManagement.Application.DTOs;
using AccountManagement.Application.Services;
using AccountManagement.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AccountManagement.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrdersController(IOrderService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _service.GetAllAsync();

            return Ok(ApiResponseFactory.Ok<IEnumerable<Order>>(
                orders,
                message: "Orders retrieved successfully",
                status: StatusCodes.Status200OK));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(long id)
        {
            var order = await _service.GetByIdAsync(id);
            //if (order == null)
            //{

            //    throw new BusinessException($"Order with id '{id}' not found.");
            //    var error = new ApiError
            //    {
            //        FieldErrors = null,
            //        GeneralErrors = [$"Order with id {id} not found."]
            //    };

            //    return NotFound(ApiResponseFactory.Failure<Order>(
            //        error,
            //        message: $"Order with id {id} not found.",
            //        status: StatusCodes.Status404NotFound));
            //}

            return Ok(ApiResponseFactory.Ok<Order>(
                order,
                message: "Order retrieved successfully",
                status: StatusCodes.Status200OK));
        }

        [HttpPost]
        public async Task<IActionResult> AddOrder(OrderDto dto)
        {
            var order = await _service.CreateAsync(dto);
            
            return Ok(ApiResponseFactory.Ok<Order>(
                order,
                message: "Order created successfully",
                status: StatusCodes.Status200OK));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(long id, OrderDto dto)
        {
            var order = await _service.UpdateAsync(id, dto);
            return Ok(ApiResponseFactory.Ok<Order>(
                order,
                message: "Order updated successfully",
                status: StatusCodes.Status200OK));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(long id)
        {
            await _service.DeleteAsync(id);
            return Ok("Order deleted successfully");
        }
    }
}
