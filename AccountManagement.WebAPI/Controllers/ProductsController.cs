using AccountManagement.Application.Common.Responses;
using AccountManagement.Application.Exceptions;
using AccountManagement.Application.Validators;
using AccountManagement.Domain.Entities;
using AccountManagement.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace AccountManagement.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController(ProductValidator validator) : ControllerBase
    {
        private readonly IValidator<Product> _validator = validator; 

        private static List<Product> products =
        [
            new() { Id = 1, Name = "RAJ", Description = "", CategoryType = "", Category = "", Price = 1.2m },
            new() { Id = 2, Name = "RIDD", Description = "", CategoryType = "", Category = "", Price = 1.2m  },
            new() { Id = 3, Name = "ROHA", Description = "", CategoryType = "", Category = "", Price = 1.2m  },
            new() { Id = 4, Name = "RADH", Description = "", CategoryType = "", Category = "", Price = 1.2m  }
        ];

        [HttpGet("error")]
        public ActionResult DisplayError()
        {
            throw new InvalidOperationException("Simulated unexpected error for testing");
        }

        [HttpGet]
        public ActionResult<List<Product>> GetProducts()
        {
            return Ok(ApiResponseFactory.Ok<IEnumerable<Product>>(
                products,
                message: "Products retrieved successfully",
                status: StatusCodes.Status200OK));
        }

        [HttpGet("{id}")]
        public ActionResult<ApiResponse<Product>> GetProduct(int id)
        {
            var product =  products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                if (product == null)
                {
                    // Example: throw a DomainException instead of returning NotFound
                    throw new DomainException($"Product with id {id} not found.");
                }

                var error = new ApiError
                {
                    FieldErrors = null,
                    GeneralErrors = [ $"Product with id {id} not found." ]
                };

                return NotFound(ApiResponseFactory.Failure<Product>(
                    error, 
                    message: $"Product with id {id} not found.", 
                    status: StatusCodes.Status404NotFound));
            }

            return Ok(ApiResponseFactory.Ok<Product>(
                product, 
                message: "Product retrieved successfully", 
                status: StatusCodes.Status200OK));
        }

        [HttpPost]
        public async Task<ActionResult<Product>> AddProduct(Product product)
        {
            var result = _validator.Validate(product);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);

            // Domain rule: price must be > 0
            if (product.Price <= 0)
                throw new DomainException("Product price must be greater than zero.");

            // Business rule: product name must be unique
            if (products.Any(p => p.Name == product.Name))
                throw new BusinessException($"Product with name '{product.Name}' already exists.");

            products.Add(product);

            return Ok(ApiResponseFactory.Ok<Product>(
                product,
                message: "Product created successfully",
                status: StatusCodes.Status200OK));
        }
    }
}
