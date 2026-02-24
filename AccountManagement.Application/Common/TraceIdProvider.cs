using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace AccountManagement.Application.Common
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
            // 1. Prefer HttpContext.TraceIdentifier (GUID style)
            var httpTraceId = _httpContextAccessor?.HttpContext?.TraceIdentifier;
            if (!string.IsNullOrEmpty(httpTraceId))
                return httpTraceId;

            // 2. Fallback to Activity.TraceId, but convert to GUID format
            var activity = Activity.Current;
            if (activity != null)
                return activity.TraceId.ToString();

            // 3. Final fallback: generate a new GUID
            return Guid.NewGuid().ToString();

        }
    }

}
