namespace AccountManagement.Application.DTOs
{
    public class OrderDto
    {
        public required string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
