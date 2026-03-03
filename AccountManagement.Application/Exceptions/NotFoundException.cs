using Microsoft.AspNetCore.Http;

namespace AccountManagement.Application.Exceptions
{
    // Base exception for business rule violations
    public class NotFoundException : Exception
    {
        public int StatusCode { get; }

        public NotFoundException(string message) : base(message)
        {
            StatusCode = StatusCodes.Status404NotFound; // default
        }

        public NotFoundException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
