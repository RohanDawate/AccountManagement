namespace AccountManagement.Application.Exceptions
{
    // Base exception for business rule violations
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message) { }
    }

}
