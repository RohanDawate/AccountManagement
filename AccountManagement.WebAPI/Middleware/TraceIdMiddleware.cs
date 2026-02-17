using Serilog.Context;

namespace AccountManagement.WebAPI.Middleware
{
    public class TraceIdMiddleware
    {
        private readonly RequestDelegate _next;

        public TraceIdMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            var traceId = context.TraceIdentifier;
            using (LogContext.PushProperty("TraceId", traceId))
            {
                await _next(context);
            }
        }
    }
}


