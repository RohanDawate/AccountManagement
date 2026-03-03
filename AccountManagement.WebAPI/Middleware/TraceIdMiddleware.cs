using Serilog.Context;
using System.Diagnostics;
using AccountManagement.Application.Common;

namespace AccountManagement.WebAPI.Middleware
{
    public class TraceIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _scopeFactory;

        public TraceIdMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using var scope = _scopeFactory.CreateScope();
            var traceIdProvider = scope.ServiceProvider.GetRequiredService<ITraceIdProvider>();

            // Get traceId from provider (Activity, HttpContext, GUID fallback)
            var traceId = traceIdProvider.GetTraceId();
            var activity = Activity.Current;
            var spanId = activity?.SpanId.ToString() ?? string.Empty;
            var parentId = activity?.ParentId ?? string.Empty;

            // Push all correlation properties into Serilog LogContext
            using (LogContext.PushProperty("TraceId", traceId))
            using (LogContext.PushProperty("SpanId", spanId))
            using (LogContext.PushProperty("ParentId", parentId))
            {
                // Also add to response headers for client-side correlation
                context.Response.Headers["X-TraceId"] = traceId;
                await _next(context);
            }

        }
    }
}
