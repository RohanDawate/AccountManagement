using AccountManagement.Application.Common.Responses;
using System.Text.Json;

namespace AccountManagement.WebAPI.Middleware
{
    public class UnifiedResponseMiddleware
    {
        private readonly RequestDelegate _next;

        public UnifiedResponseMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var bodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            context.Response.Body = originalBodyStream;
            var statusCode = context.Response.StatusCode;
            var traceId = context.TraceIdentifier;

            object? data = null;
            if (!string.IsNullOrWhiteSpace(bodyText))
            {
                //try
                //{
                //    if (statusCode >= 400)
                //    {
                //        // Deserialize directly into ApiError
                //        data = JsonSerializer.Deserialize<ApiError>(bodyText);
                //    }
                //    else
                //    {
                //        // For success, deserialize into a generic object
                //        data = JsonSerializer.Deserialize<object>(bodyText);
                //    }
                //}
                //catch
                //{
                //    data = bodyText; // fallback if not JSON
                //}

                string? message = null;

                try
                {
                    var parsed = JsonSerializer.Deserialize<JsonElement>(bodyText);

                    if (parsed.TryGetProperty("message", out var msgElement) && msgElement.ValueKind == JsonValueKind.String)
                    {
                        message = msgElement.GetString();
                    }

                    data = parsed.TryGetProperty("data", out var dataElement) ? dataElement : parsed;
                }
                catch
                {
                    data = bodyText;
                }
            }

            ApiResponse<object> response;
            if (statusCode >= 200 && statusCode < 300)
            {
                // ✅ Success
                response = ApiResponse<object>.Ok(data, message: "", traceId: traceId);
            }
            else if (statusCode >= 300 && statusCode < 400)
            {
                // ✅ Redirection
                response = ApiResponse<object>.Failure(
                    status: statusCode,
                    message: "Redirection occurred",
                    error: null, //new { location = context.Response.Headers["Location"].ToString() },
                    traceId: traceId
                );
            }
            else if (statusCode >= 400 && statusCode < 500)
            {
                ApiError? errorResponse = data as ApiError ?? new ApiError
                {
                    FieldErrors = null,
                    GeneralErrors = new List<string> { "The requested resource was not found." }
                };

                // ✅ Prefer controller’s message
                string message = errorResponse.GeneralErrors != null && errorResponse.GeneralErrors.Any()
                    ? string.Join("; ", errorResponse.GeneralErrors)
                    : statusCode switch
                    {
                        StatusCodes.Status400BadRequest => "Validation failed",
                        StatusCodes.Status401Unauthorized => "Authentication required",
                        StatusCodes.Status403Forbidden => "Access denied",
                        StatusCodes.Status404NotFound => "The requested resource was not found",
                        StatusCodes.Status429TooManyRequests => "Too many requests, please try again later",
                        _ => $"A client error occurred (status {statusCode})"
                    };

                response = ApiResponse<object>.Failure(
                    status: statusCode,
                    message: message,
                    error: errorResponse,
                    traceId: traceId
                );

            }
            else if (statusCode >= 500)
            {
                // ✅ Server errors
                var errorResponse = new ApiError
                {
                    FieldErrors = null, // no field-specific errors for 404
                    GeneralErrors = new List<string>() 
                    {
                        "An unexpected server error occurred"
                    }
                };

                response = ApiResponse<object>.Failure(
                    status: statusCode,
                    message: "An unexpected server error occurred",
                    error: errorResponse,
                    traceId: traceId
                );
            }
            else
            {
                // ✅ Fallback
                response = ApiResponse<object>.Failure(
                    status: statusCode,
                    message: $"Unexpected status code {statusCode}",
                    error: null,
                    traceId: traceId
                );
            }

            await context.Response.WriteAsJsonAsync(response);
        }

    }
}
