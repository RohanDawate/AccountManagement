using Serilog;
using System.Text;
using AccountManagement.WebAPI.Extensions;

namespace AccountManagement.WebAPI.Middleware
{
 
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.EnableBuffering();

            // Capture headers
            var headers = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());

            // Capture request body
            string requestBody = "";
            if (context.Request.ContentLength > 0)
            {
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            // Capture response
            var originalBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;
                       
            await _next(context);

            responseBodyStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(originalBodyStream);

            var logEntry = new ApiLogEntry
            {
                Timestamp = DateTime.UtcNow,
                Endpoint = $"{context.Request.Method} {context.Request.Path}",
                Headers = headers,
                RequestBody = requestBody,
                ResponseBody = responseBody,
                StatusCode = context.Response.StatusCode,
                Message = "Request processed",
                StackTrace = null,
                IsSuccess = context.Response.StatusCode < 400
            };

            Log.Information("{@ApiLogEntry}", logEntry);
            
            context.Response.Body = originalBodyStream;

        }
    }

}
