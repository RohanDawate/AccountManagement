namespace AccountManagement.Domain.Exceptions
{
    // Base exception for domain model errors
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
    }

}
