using AccountManagement.Application.Common.Responses;
using AccountManagement.WebAPI.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace AccountManagement.WebAPI.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Serilog.ILogger _logger;

        public ExceptionMiddleware(RequestDelegate next, Serilog.ILogger logger) 
        { 
            _next = next; 
            _logger = logger; 
        }

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
                    error: errorResponse,
                    message: "Validation failed",
                    status: StatusCodes.Status400BadRequest,                    
                    traceId: traceId
                );

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(response);
            }
            catch (Exception ex)
            {
                context.Response.Body = originalBodyStream; // ✅ restore

                // Capture request details
                var endpoint = context.GetEndpoint(); 
                var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>(); 
                var controllerName = actionDescriptor?.ControllerName; 
                var actionName = actionDescriptor?.ActionName;

                // Store exception for unified logging
                context.Items["Exception"] = ex;
                context.Items["ExceptionController"] = controllerName;
                context.Items["ExceptionAction"] = actionName;
                context.Items["ExceptionMethodName"] = ex.TargetSite?.Name;
                
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

                // ✅ Explicitly set status code before writing
                context.Response.StatusCode = StatusCodes.Status500InternalServerError; 
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(response);

            }
        }
    }

}
