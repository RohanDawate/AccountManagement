using AccountManagement.Application.Common.Responses;
using System.Text.Json;

namespace AccountManagement.WebAPI.Middleware
{

    public class ApiResponseMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiResponseMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var bodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            context.Response.Body = originalBodyStream; // ✅ restore
            var statusCode = context.Response.StatusCode;

            // Deserialize the original body into an object
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

            ApiResponse<object> response = null;
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300) 
            { 
                if (data is null) 
                { 
                    var errorResponse = new ApiError { GeneralErrors = new List<string> { "Resource not found" } }; 
                    
                    response = ApiResponse<object>.Failure(error: errorResponse, message: "Resource not found", 
                        status: StatusCodes.Status404NotFound, traceId: context.TraceIdentifier); 
                    
                    context.Response.StatusCode = StatusCodes.Status404NotFound; } 
                
                else { 
                    response = ApiResponse<object>.Ok(data: data, status: context.Response.StatusCode, 
                        traceId: context.TraceIdentifier); } 
            }

            await context.Response.WriteAsJsonAsync(response);
        }
    }

}
