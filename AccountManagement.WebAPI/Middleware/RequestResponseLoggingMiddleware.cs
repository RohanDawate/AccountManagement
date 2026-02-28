using AccountManagement.WebAPI.Extensions;
using AccountManagement.WebAPI.Logging;
using Microsoft.AspNetCore.Mvc.Controllers;
using Serilog;
using System.Text;
using System.Text.Json;

namespace AccountManagement.WebAPI.Middleware
{
 
    public class RequestResponseLoggingMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.EnableBuffering();

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
            var statusCode = context.Response.StatusCode;

            // Capture stack trace if ExceptionMiddleware stored it
            var exception = context.Items["Exception"] as Exception;
            var errorType = context.Items["ErrorType"]?.ToString();
            var operation = context.Items["Operation"]?.ToString();

            // Capture controller message if any
            string? ctrlMessage = null;
            if (!string.IsNullOrWhiteSpace(responseBody))
            {
                try
                {
                    using var doc = JsonDocument.Parse(responseBody);
                    var root = doc.RootElement;

                    if (root.ValueKind == JsonValueKind.Object &&
                        root.TryGetProperty("message", out var messageElement))
                    {
                        ctrlMessage = messageElement.GetString();
                    }
                    else if (root.TryGetProperty("title", out var titleElement))
                    {
                        // Use "title" if "message" is missing (common in RFC error responses)
                        ctrlMessage = titleElement.GetString();
                    }

                    if (root.TryGetProperty("title", out var titleElementForError))
                        errorType = titleElementForError.GetString();
                }
                catch (JsonException)
                {
                    // Response body was not valid JSON (e.g., plain text or HTML error page)
                    ctrlMessage = null;
                }
            }

            string message;
            if (!string.IsNullOrEmpty(ctrlMessage))
            {
                // Case 1: Use the message from ApiResponse
                message = ctrlMessage;
            }
            else if (exception != null)
            {
                // Case 2: Exception occurred
                message = $"Error: {exception.Message}";
            }
            else if (statusCode >= 400)
            {
                // Fallback for error responses without "message"
                message = $"Request failed with status code {statusCode}";
                errorType ??= $"HTTP {statusCode}";
            }
            else
            {
                // Case 3: Default fallback
                message = $"{controllerName}.{actionName} completed successfully";
            }

            var logEntry = new ApiLogEntry
            {
                Timestamp = DateTime.Now,
                TraceId = context.TraceIdentifier,
                Operation = operation,
                Endpoint = $"{context.Request.Method} {context.Request.Path}",
                Headers = RequestSanitizer.SanitizeHeaders(context.Request.Headers),
                Query = RequestSanitizer.SanitizeQuery(context.Request.QueryString.Value),
                RequestBody = RequestSanitizer.SanitizeBody(requestBody),
                ResponseBody = RequestSanitizer.SanitizeBody(responseBody),
                IsSuccess = context.Response.StatusCode < 400,
                StatusCode = statusCode,
                Message = message,
                ErrorType = errorType,
                ExceptionType = exception?.GetType().Name,
                StackTrace = exception != null ? LoggingExtensions.BuildCleanStackTrace(exception) : null,
            };

            // Choose log level based on status code
            if (statusCode >= 500)
                Log.Error("{@ApiLogEntry}", logEntry);
            else if (statusCode >= 400)
                Log.Warning("{@ApiLogEntry}", logEntry);
            else
                Log.Information("{@ApiLogEntry}", logEntry);

            context.Response.Body = originalBodyStream;

        }
    }

}
