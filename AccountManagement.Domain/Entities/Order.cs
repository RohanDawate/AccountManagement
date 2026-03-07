namespace AccountManagement.Domain.Entities
{
    public class Order
    {
        private static long _lastId = 0;

        public long Id { get; private set; }
        public string CustomerName { get; private set; }
        public decimal TotalAmount { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public Order(string customerName, decimal totalAmount)
        {
            Validate(customerName, totalAmount);

            Id = GenerateId();
            CustomerName = customerName;
            TotalAmount = totalAmount;
            CreatedAt = DateTime.UtcNow;
        }

        public void Update(string customerName, decimal totalAmount)
        {
            Validate(customerName, totalAmount);

            CustomerName = customerName;
            TotalAmount = totalAmount;
        }

        private static void Validate(string customerName, decimal totalAmount)
        {
            if (string.IsNullOrWhiteSpace(customerName))
                throw new ArgumentException("Customer name is required");
            if (totalAmount <= 0)
                throw new ArgumentException("Total amount must be positive");
        }

        private static long GenerateId()
        {
            return Interlocked.Increment(ref _lastId);
        }
    }
}
