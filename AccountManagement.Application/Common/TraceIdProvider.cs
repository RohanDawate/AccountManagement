using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace AccountManagement.Application.Common.Tracing
{
    public interface ITraceIdProvider
    {
        string GetTraceId();
    }

    public class TraceIdProvider : ITraceIdProvider
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public TraceIdProvider(IHttpContextAccessor? httpContextAccessor = null)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetTraceId()
        {
            // Prefer distributed tracing (Activity)
            var activity = Activity.Current;
            if (activity != null)
                return activity.TraceId.ToString();

            // Use HttpContext.TraceIdentifier if available
            var traceId = _httpContextAccessor?.HttpContext?.TraceIdentifier;
            if (!string.IsNullOrEmpty(traceId))
                return traceId;

            // Fallback for batch jobs / Control-M jobs
            return Guid.NewGuid().ToString();
        }
    }

}
