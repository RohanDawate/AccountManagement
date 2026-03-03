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
            Id = GenerateId();
            CustomerName = customerName;
            TotalAmount = totalAmount;
            CreatedAt = DateTime.UtcNow;
        }

        public void Update(string customerName, decimal totalAmount)
        {
            CustomerName = customerName;
            TotalAmount = totalAmount;
        }

        private static long GenerateId()
        {
            return Interlocked.Increment(ref _lastId);
        }
    }
}
