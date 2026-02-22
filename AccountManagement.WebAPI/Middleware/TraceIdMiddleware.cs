using Serilog.Context;
using System.Diagnostics;

namespace AccountManagement.WebAPI.Middleware
{
    public class TraceIdMiddleware
    {
        private readonly RequestDelegate _next;

        public TraceIdMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            var activity = Activity.Current;

            // Prefer distributed tracing IDs if available
            var traceId = activity?.TraceId.ToString() ?? context.TraceIdentifier;
            var spanId = activity?.SpanId.ToString();
            var parentId = activity?.ParentId;

            using (LogContext.PushProperty("TraceId", traceId))
            using (LogContext.PushProperty("SpanId", spanId ?? string.Empty))
            using (LogContext.PushProperty("ParentId", parentId ?? string.Empty))
            {
                await _next(context);
            }
        }
    }
}
