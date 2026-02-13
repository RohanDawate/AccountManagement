using Microsoft.AspNetCore.Mvc;

namespace AccountManagement.Application.Common.Responses
{

    public class ApiResponse<T>
    {

        public bool Success { get; set; }
        public T? Data { get; set; }
        public ProblemDetails? Error { get; set; }
        public string? TraceId { get; set; } 

        public static ApiResponse<T> Ok(T data, string? traceId = null) =>
            new ApiResponse<T> { Success = true, Data = data, TraceId = traceId };

        public static ApiResponse<T> Failure(string title, string detail, int status = 400, string? traceId = null) =>
            new ApiResponse<T>
            {
                Success = false,
                Error = new ProblemDetails
                {
                    Title = title,
                    Detail = detail,
                    Status = status
                },
                TraceId = traceId

            };
    }



}
