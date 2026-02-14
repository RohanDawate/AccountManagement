using Microsoft.AspNetCore.Mvc;

namespace AccountManagement.Application.Common.Responses
{
    public class ApiResponse<T>
    {

        public bool Success { get; set; }
        public int Status { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public ApiError? Error { get; set; }
        public string? TraceId { get; set; }
        

        public static ApiResponse<T> Ok(T data, string message = "Request successful", int status = 200, string? traceId = null) 
        { 
            return new ApiResponse<T> 
            { 
                Success = true,
                Status = status,
                Message = message ?? "Request processed successfully",
                Data = data, 
                Error = null, 
                TraceId = traceId               
            }; 
        }
        
        public static ApiResponse<T> Failure(ApiError error, string message = "Request failed", int status = 400, string? traceId = null) 
        { 
            return new ApiResponse<T> 
            { 
                Success = false,
                Status = status,
                Message = message ?? "Request failed",
                Data = default, 
                Error = error, 
                TraceId = traceId
            }; 
        }
    }



}
