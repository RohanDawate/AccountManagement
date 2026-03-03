using AccountManagement.Application.Common;
using AccountManagement.Application.Common.Responses;
using AccountManagement.Application.Exceptions;
using AccountManagement.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.WebUtilities;

namespace AccountManagement.WebAPI.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Serilog.ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public ExceptionMiddleware(RequestDelegate next, Serilog.ILogger logger, IServiceScopeFactory scopeFactory)
        {
            _next = next; 
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task Invoke(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;

            try
            {
                await _next(context);

                //// Handle framework-generated errors (e.g., 404, 415) if no exception was thrown
                //if (context.Response.StatusCode >= 400 && !context.Response.HasStarted)
                //{
                //    await WriteErrorResponseAsync(context,
                //        statusCode: context.Response.StatusCode,
                //        message: ReasonPhrases.GetReasonPhrase(context.Response.StatusCode),
                //        errorType: "HttpError",
                //        errors: new List<string> { $"HTTP {context.Response.StatusCode}" });
                //}
            }            
            catch (Exception ex)
            {
                context.Response.Body = originalBodyStream; // restore

                // Capture request details
                var endpoint = context.GetEndpoint(); 
                var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>(); 
                var controller = actionDescriptor?.ControllerName ?? "UnknownController";
                var action = actionDescriptor?.ActionName ?? "UnknownAction";
                var operation = $"{controller}.{action}";

                // Resolve ITraceIdProvider per request
                using var scope = _scopeFactory.CreateScope();
                var traceIdProvider = scope.ServiceProvider.GetRequiredService<ITraceIdProvider>();
                var traceId = traceIdProvider.GetTraceId();

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
                        statusCode =  bex.StatusCode;
                        message = "Business rule violation";
                        errorType = "Business";
                        errorResponse = new ApiError
                        {
                            GeneralErrors = new List<string> { bex.Message }
                        };
                        break;

                    case NotFoundException nfex:
                        statusCode = nfex.StatusCode;
                        message = "Business rule violation";
                        errorType = "Business";
                        errorResponse = new ApiError
                        {
                            GeneralErrors = new List<string> { nfex.Message }
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

                // Explicitly set status code before writing
                context.Response.Clear(); // prevent duplicate payloads
                context.Response.StatusCode = statusCode; 
                await context.Response.WriteAsJsonAsync(response);
            }
        }

        //private static async Task WriteErrorResponseAsync(HttpContext context,
        //    int statusCode,
        //    string message,
        //    string errorType,
        //    List<string> errors)
        //{
        //    var response = ApiResponse<string>.Failure(
        //        status: statusCode,
        //        message: message,
        //        error: new ApiError { GeneralErrors = errors },
        //        traceId: context.TraceIdentifier
        //    );

        //    context.Response.Clear(); // prevent duplicate payloads
        //    context.Response.StatusCode = statusCode;
        //    context.Response.ContentType = "application/json";
        //    await context.Response.WriteAsJsonAsync(response);
        //}

    }

}
