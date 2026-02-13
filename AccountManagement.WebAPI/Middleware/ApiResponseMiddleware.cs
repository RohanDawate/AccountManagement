using AccountManagement.Application.Common.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
                // ✅ Client error
                apiResponse = ApiResponse<object>.Failure(
                    "Client Error",
                    $"A client error occurred (status {statusCode}).",
                    statusCode,
                    context.TraceIdentifier
                );

                // include original body if available
                if (data != null)
                    apiResponse.Error!.Extensions["details"] = data;
            }
            else if (statusCode >= 500)
            {
                // ✅ Server error
                apiResponse = ApiResponse<object>.Failure(
                    "Server Error",
                    $"An unexpected server error occurred (status {statusCode}).",
                    statusCode,
                    context.TraceIdentifier
                );

                if (data != null)
                    apiResponse.Error!.Extensions["details"] = data;
            }
            else
            {
                // fallback
                apiResponse = ApiResponse<object>.Failure(
                    "Unknown Status",
                    $"Unexpected status code {statusCode}.",
                    statusCode,
                    context.TraceIdentifier
                );
            }

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(apiResponse));

            //if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            //{
            //    var apiResponse = ApiResponse<object>.Ok(data, context.TraceIdentifier);
            //    //apiResponse.Success = true;
            //    //apiResponse.Error = null;

            //    context.Response.ContentType = "application/json";
            //    await context.Response.WriteAsync(JsonSerializer.Serialize(apiResponse));
            //}
            //else
            //{
            //    await responseBody.CopyToAsync(originalBodyStream);
            //    context.Response.Body = originalBodyStream;
            //}
        }
    }

}
