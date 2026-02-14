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

        [HttpGet]
        public ActionResult<List<Product>> GetProducts() => Ok(products);

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product =  products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                var error = new ApiError
                {
                    FieldErrors = null,
                    GeneralErrors = new List<string>
                    {
                        "The requested product with id {{id}} was not found."
                    }
                };

                return NotFound(error); 
            }

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> AddProduct(Product product)
        {
            var result = _validator.Validate(product);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);

            products.Add(product);

            return Ok(product);
        }
    }
}
