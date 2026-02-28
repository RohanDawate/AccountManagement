using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace AccountManagement.Application.Features.Products.Commands.AddProduct
{
    public class AddProductValidator : AbstractValidator<AddProductCommand>
    {
        public AddProductValidator()
        {

            // Root-level null check
            RuleFor(p => p)
                .NotNull().WithMessage("Product object must not be null");

            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(100).WithMessage("Product name must be less than 100 characters.");

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
