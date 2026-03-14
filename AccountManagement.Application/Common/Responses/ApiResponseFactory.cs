namespace AccountManagement.Application.Common.Responses
{
    public static class ApiResponseFactory
    {
        private static ITraceIdProvider? _traceIdProvider;

        public static void Configure(ITraceIdProvider provider)
        {
            _traceIdProvider = provider;
        }

        private static string GetTraceId()
        {
            return _traceIdProvider?.GetTraceId() ?? Guid.NewGuid().ToString();
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
