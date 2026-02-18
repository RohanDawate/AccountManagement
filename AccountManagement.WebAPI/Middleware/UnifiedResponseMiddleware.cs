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

            ApiResponse<object>? apiResponse = null;
            try
            {
                apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(bodyText);
                if (apiResponse != null)
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(bodyText);
                    return;
                }
            }
            catch
            {
                // fallback if not ApiResponse
            }

            // ✅ Otherwise wrap raw data into ApiResponse<T>       
            ApiResponse<object> response; 
            if (statusCode >= 200 && statusCode < 300) 
            { 
                response = ApiResponse<object>.Ok(bodyText, "Request successful", statusCode, traceId); 
            }
            else if (statusCode >= 400 && statusCode < 500) 
            { 
                var error = new ApiError 
                { 
                    GeneralErrors = new List<string> { bodyText } 
                }; 
                
                response = ApiResponse<object>.Failure(error, message: "Client error", status: statusCode, traceId: traceId); 
            }
            else if (statusCode >= 500) 
            { 
                var error = new ApiError 
                { 
                    GeneralErrors = new List<string> { "An unexpected server error occurred" } 
                }; 
                
                response = ApiResponse<object>.Failure(error, message: "Server error", status: statusCode, traceId: traceId); 
            }
            else 
            { 
                var error = new ApiError 
                { 
                    GeneralErrors = new List<string> { bodyText } 
                };
                
                response = ApiResponse<object>.Failure(error, "Request failed", statusCode, traceId); 
            } 
            
            context.Response.ContentType = "application/json"; 
            await context.Response.WriteAsJsonAsync(response);
        }

    }
}
