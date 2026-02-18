using AccountManagement.Application.Common.Responses;
using AccountManagement.Application.Validators;
using AccountManagement.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace AccountManagement.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IValidator<Product> _validator;

        public ProductsController(ProductValidator validator) 
        { 
            _validator = validator; 
        }

        private static List<Product> products = new List<Product>
        {
            new Product { Id = 1, Name = "RAJ", Description = "", CategoryType = "", Category = "", Price = 1.2m },
            new Product { Id = 2, Name = "RIDD", Description = "", CategoryType = "", Category = "", Price = 1.2m  },
            new Product { Id = 3, Name = "ROHA", Description = "", CategoryType = "", Category = "", Price = 1.2m  },
            new Product { Id = 4, Name = "RADH", Description = "", CategoryType = "", Category = "", Price = 1.2m  }
        };

        [HttpGet("error")]
        public ActionResult DisplayError()
        {
            throw new InvalidOperationException("Simulated unexpected error for testing");
        }

        [HttpGet]
        public ActionResult<List<Product>> GetProducts()
        {
            return Ok(ApiResponse<IEnumerable<Product>>.Ok(
                products,
                message: "Products retrieved successfully",
                status: 200,
                traceId: HttpContext.TraceIdentifier));
        }

        [HttpGet("{id}")]
        public ActionResult<ApiResponse<Product>> GetProduct(int id)
        {
            var product =  products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                var error = new ApiError
                {
                    FieldErrors = null,
                    GeneralErrors = new List<string> { $"Product with id {id} not found." }
                };

                return NotFound(ApiResponse<Product>.Failure(error, $"Product with id {id} not found.", 404, HttpContext.TraceIdentifier));
            }

            return Ok(ApiResponse<Product>.Ok(
                product, 
                message: "Product retrieved successfully", 
                status: 200, 
                traceId: HttpContext.TraceIdentifier));
        }

        [HttpPost]
        public async Task<ActionResult<Product>> AddProduct(Product product)
        {
            var result = _validator.Validate(product);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);

            products.Add(product);

            return Ok(ApiResponse<Product>.Ok(
                product,
                message: "Product created successfully",
                status: 200,
                traceId: HttpContext.TraceIdentifier));
        }
    }
}
