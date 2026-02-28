using AccountManagement.Domain.Entities;
using FluentValidation;

namespace AccountManagement.Application.Validators
{
    public class ProductValidator : AbstractValidator<Product>
    {

        public ProductValidator() 
        {
            // Root-level null check
            RuleFor(p => p)
                .NotNull()
                .WithMessage("Product cannot be empty");

            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must be less than 100 characters.");

            RuleFor(p => p.CategoryType)
                .NotEmpty().WithMessage("Account Type is required.")
                .Must(type => type == "Retail" || type == "Household" || type == "Commercial")
                    .WithMessage("Account Type must be either 'Retail', 'Household', or 'Commercial'.");
            
            RuleFor(p => p.Category)
                .NotEmpty().WithMessage("The Category field is required.")
                .MaximumLength(50).WithMessage("Category must be less than 50 characters.");

            RuleFor(p => p.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.");

        }
    }
}
