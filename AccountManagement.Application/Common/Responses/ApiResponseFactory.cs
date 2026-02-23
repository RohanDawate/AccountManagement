using Microsoft.AspNetCore.Http;

namespace AccountManagement.Application.Common.Responses
{
    public static class ApiResponseFactory
    {
        private static IHttpContextAccessor? _httpContextAccessor;

        public static void Configure(IHttpContextAccessor accessor)
        {
            _httpContextAccessor = accessor;
        }

        private static string GetTraceId()
        {
            return _httpContextAccessor?.HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
        }

        public static ApiResponse<T> Ok<T>(T data, string message, int status)
        {
            return ApiResponse<T>.Ok(
                data,
                message: message,
                status: status,
                traceId: GetTraceId()
            );
        }

        public static ApiResponse<T> Failure<T>(ApiError error, string message, int status)
        {
            return ApiResponse<T>.Failure(
                error: error,
                message: message,
                status: status,
                traceId: GetTraceId()
            );
        }
    }

}
