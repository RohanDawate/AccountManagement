namespace AccountManagement.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = null!;
        public string CategoryType { get; set; } = null!;
        public string Category { get; set; } = null!;
        public decimal Price { get; set; }
        
        public Product() { } // for object initializer

        public Product(int id, string name, string description, string categoryType, string category, decimal price)
        {
            if (price <= 0)
                throw new ArgumentException("Price must be greater than zero."); // domain guard

            Id = id;
            Name = name;
            Description = description;
            CategoryType = categoryType;    
            Category = category;
            Price = price;
        }

    }
}
