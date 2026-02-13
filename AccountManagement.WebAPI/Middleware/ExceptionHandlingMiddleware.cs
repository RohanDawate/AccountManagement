using AccountManagement.Application.Common.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using FluentValidation;

namespace AccountManagement.WebAPI.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;

            try
            {
                await _next(context);
            }
            catch (ValidationException ex) 
            {
                context.Response.Body = originalBodyStream; // ✅ restore

                var errors = ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                var apiResponse = ApiResponse<object>.Failure(
                                        "Validation failed",
                                        "One or more validation errors occurred.",
                                        StatusCodes.Status400BadRequest,
                                        context.TraceIdentifier
);

                // ✅ Only include the errors dictionary, not the full ProblemDetails
                apiResponse.Error!.Extensions["errors"] = errors;

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(apiResponse));


                //var problem = new ValidationProblemDetails(errors)
                //{
                //    Type = "https://httpstatuses.com/400",
                //    Title = "Validation failed", 
                //    Status = StatusCodes.Status400BadRequest,
                //    Detail = "One or more validation errors occurred.",
                //    Instance = context.Request.Path 
                //}; 
                
                //context.Response.StatusCode = problem.Status.Value; 
                //context.Response.ContentType = "application/problem+json";

                //var apiResponse = ApiResponse<object>.Failure(problem.Title!, problem.Detail!, problem.Status!.Value, context.TraceIdentifier);
                //apiResponse.Error!.Extensions["errors"] = errors;

                //await context.Response.WriteAsync(JsonSerializer.Serialize(apiResponse)); 
            }
            catch (Exception ex)
            {
                context.Response.Body = originalBodyStream; // ✅ restore

                var problem = new ProblemDetails
                {
                    Type = "https://httpstatuses.com/500",
                    Title = "An unexpected error occurred",
                    Status = (int)HttpStatusCode.InternalServerError,
                    Detail = ex.Message,
                    Instance = context.Request.Path
                };

                context.Response.StatusCode = problem.Status.Value;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(ApiResponse<string>.Failure(problem.Title!, problem.Detail!, problem.Status!.Value));
            }
        }
    }

}
