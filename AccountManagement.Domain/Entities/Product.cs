using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
