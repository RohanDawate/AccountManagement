using AccountManagement.Application.Common.Responses;
using FluentValidation;

namespace AccountManagement.WebAPI.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;

            try
            {
                await _next(context);
            }
            catch (ValidationException vex)
            {
                context.Response.Body = originalBodyStream; // ✅ restore

                var errorResponse = new ApiError
                {
                    FieldErrors = vex.Errors.GroupBy(e => e.PropertyName).ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    )
                };

                var traceId = context.TraceIdentifier;
                var response = ApiResponse<object>.Failure(
                    status: StatusCodes.Status400BadRequest,
                    message: "Validation failed",
                    error: errorResponse,
                    traceId: traceId
                );

                await context.Response.WriteAsJsonAsync(response);
            }
            catch (Exception ex)
            {
                context.Response.Body = originalBodyStream; // ✅ restore

                var errorResponse = new ApiError
                {
                    GeneralErrors = new List<string> { ex.Message }
                };

                var traceId = context.TraceIdentifier;
                var response = ApiResponse<string>.Failure(
                    status: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred",
                    error: errorResponse,
                    traceId: traceId
                );

                await context.Response.WriteAsJsonAsync(response);

            }
        }
    }

}
