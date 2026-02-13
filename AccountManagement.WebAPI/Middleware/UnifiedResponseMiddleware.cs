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
            object? data = null;

            if (!string.IsNullOrWhiteSpace(bodyText))
            {
                try
                {
                    data = JsonSerializer.Deserialize<object>(bodyText);
                }
                catch
                {
                    data = bodyText;
                }
            }

            ApiResponse<object> apiResponse;

            if (statusCode >= 200 && statusCode < 300)
            {
                // ✅ Success
                apiResponse = ApiResponse<object>.Ok(data, context.TraceIdentifier);
            }
            else if (statusCode >= 300 && statusCode < 400)
            {
                // ✅ Redirection
                apiResponse = ApiResponse<object>.Failure(
                    "Redirection",
                    $"Request was redirected (status {statusCode}).",
                    statusCode,
                    context.TraceIdentifier
                );
            }
            else if (statusCode >= 400 && statusCode < 500)
            {
                string title;
                string detail;

                switch (statusCode)
                {
                    case StatusCodes.Status400BadRequest:
                        title = "Bad Request";
                        detail = "The request could not be understood or was invalid.";
                        break;
                    case StatusCodes.Status401Unauthorized:
                        title = "Unauthorized";
                        detail = "Authentication is required to access this resource.";
                        break;
                    case StatusCodes.Status403Forbidden:
                        title = "Forbidden";
                        detail = "You do not have permission to access this resource.";
                        break;
                    case StatusCodes.Status404NotFound:
                        title = "Not Found";
                        detail = "The requested resource was not found.";
                        break;
                    default:
                        title = "Client Error";
                        detail = $"A client error occurred (status {statusCode}).";
                        break;
                }

                apiResponse = ApiResponse<object>.Failure(title, detail, statusCode, context.TraceIdentifier);

                if (data != null)
                    apiResponse.Error!.Extensions["errors"] = data;
            }
            else if (statusCode >= 500)
            {
                string title = "Server Error";
                string detail = "An unexpected server error occurred.";

                apiResponse = ApiResponse<object>.Failure(title, detail, statusCode, context.TraceIdentifier);

                if (data != null)
                    apiResponse.Error!.Extensions["details"] = data;
            }
            else
            {
                apiResponse = ApiResponse<object>.Failure(
                    "Unknown Status",
                    $"Unexpected status code {statusCode}.",
                    statusCode,
                    context.TraceIdentifier
                );
            }

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(apiResponse));
        }

    }
}
