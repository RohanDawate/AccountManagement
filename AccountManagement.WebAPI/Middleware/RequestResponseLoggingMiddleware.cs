using AccountManagement.WebAPI.Extensions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Serilog;
using System.Text;

namespace AccountManagement.WebAPI.Middleware
{
 
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestResponseLoggingMiddleware(RequestDelegate next) => _next = next;

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

            // Capture details from the context object
            var endpoint = context.GetEndpoint(); 
            var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>(); 
            var controllerName = actionDescriptor?.ControllerName; 
            var actionName = actionDescriptor?.ActionName;

            // Capture stack trace if ExceptionMiddleware stored it
            var exception = context.Items["Exception"] as Exception;

            var stackTrace = context.Items["ExceptionStackTrace"]?.ToString();
            var exceptionController = context.Items["ExceptionController"]?.ToString() ?? controllerName; 
            var exceptionAction = context.Items["ExceptionAction"]?.ToString() ?? actionName; 
            var methodName = context.Items["ExceptionMethodName"]?.ToString();

            var logEntry = new ApiLogEntry
            {
                Timestamp = DateTime.Now,
                Endpoint = $"{context.Request.Method} {context.Request.Path}",
                Headers = headers,
                RequestBody = requestBody,
                ResponseBody = responseBody,
                StatusCode = context.Response.StatusCode,
                Message = exception != null ? "Unhandled exception" : "Request processed",
                StackTrace = exception?.ToString(),
                IsSuccess = context.Response.StatusCode < 400
            };

            Log.Information("{@ApiLogEntry}", logEntry);
            
            context.Response.Body = originalBodyStream;

        }
    }

}
