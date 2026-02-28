namespace AccountManagement.Application.Features.Products.Commands.AddProduct
{
    public record AddProductCommand(string Name, string Description, string CategoryType, string Category, decimal Price);
}
