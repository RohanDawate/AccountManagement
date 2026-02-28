using Microsoft.AspNetCore.Http;

namespace AccountManagement.Application.Exceptions
{
    // Base exception for business rule violations
    public class BusinessException : Exception
    {
        public int StatusCode { get; }

        public BusinessException(string message) : base(message)
        {
            StatusCode = StatusCodes.Status409Conflict; // default
        }

        public BusinessException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }


}
