namespace AccountManagement.WebAPI.Logging
{
    public class ApiLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string? TraceId { get; set; }
        public string? Operation { get; set; }
        public string? Endpoint { get; set; }
        public IDictionary<string, string>? Headers { get; set; }
        public IDictionary<string, string>? Query { get; set; }
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string? RequestBody { get; set; }
        public string? ResponseBody { get; set; }
        public string? Message { get; set; }
        public string? ErrorType { get; set; }
        public string? ExceptionType { get; set; }
        public string? StackTrace { get; set; }
    }
}
