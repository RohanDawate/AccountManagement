using AccountManagement.Application.Common.Responses;
using AccountManagement.Application.Exceptions;
using AccountManagement.Domain.Exceptions;
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
            catch (Exception ex)
            {
                context.Response.Body = originalBodyStream; // ✅ restore

                // Capture request details
                var endpoint = context.GetEndpoint(); 
                var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>(); 
                var controller = actionDescriptor?.ControllerName ?? "UnknownController";
                var action = actionDescriptor?.ActionName ?? "UnknownAction";
                var operation = $"{controller}.{action}";
                var traceId = context.TraceIdentifier;

                int statusCode;
                string message;
                string errorType;
                ApiError errorResponse;

                switch (ex)
                {
                    case ValidationException vex:
                        statusCode = StatusCodes.Status400BadRequest;
                        message = "Validation failed";
                        errorType = "Validation";
                        errorResponse = new ApiError
                        {
                            FieldErrors = vex.Errors.GroupBy(e => e.PropertyName).ToDictionary(
                                g => g.Key,
                                g => g.Select(e => e.ErrorMessage).ToArray()
                            )
                        };
                        break;

                    case DomainException dex:
                        statusCode = StatusCodes.Status422UnprocessableEntity;
                        message = "Domain rule violation";
                        errorType = "Domain";
                        errorResponse = new ApiError
                        {
                            GeneralErrors = new List<string> { dex.Message }
                        };
                        break;

                    case BusinessException bex:
                        statusCode = StatusCodes.Status409Conflict;
                        message = "Business rule violation";
                        errorType = "Business";
                        errorResponse = new ApiError
                        {
                            GeneralErrors = new List<string> { bex.Message }
                        };
                        break;

                    default:
                        statusCode = StatusCodes.Status500InternalServerError;
                        message = "Unexpected system error";
                        errorType = "System";
                        errorResponse = new ApiError
                        {
                            GeneralErrors = new List<string> { ex.Message }
                        };
                        break;
                }

                // Store exception for unified logging
                context.Items["Exception"] = ex;
                context.Items["ErrorType"] = errorType;
                context.Items["Operation"] = operation;

                var response = ApiResponse<string>.Failure(
                    status: statusCode,
                    message: message,
                    error: errorResponse,
                    traceId: traceId
                );

                // ✅ Explicitly set status code before writing
                context.Response.StatusCode = statusCode; 
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }

}
