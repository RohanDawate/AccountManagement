using AccountManagement.Application.Common;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AccountManagement.WebAPI.Logging
{
    public class LoggingActionFilter : IActionFilter
    {
        private readonly ILogger<LoggingActionFilter> _logger;
        private readonly ITraceIdProvider _traceIdProvider;

        public LoggingActionFilter(ILogger<LoggingActionFilter> logger, ITraceIdProvider traceIdProvider)
        {
            _logger = logger;
            _traceIdProvider = traceIdProvider;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var traceId = _traceIdProvider.GetTraceId();

            _logger.LogInformation(
                "Entering {Class}.{Method} TraceId={TraceId}",
                context.Controller.GetType().Name,
                context.ActionDescriptor.RouteValues["action"],
                traceId
            );
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            var traceId = _traceIdProvider.GetTraceId();

            _logger.LogInformation(
                "Exiting {Class}.{Method} TraceId={TraceId}",
                context.Controller.GetType().Name,
                context.ActionDescriptor.RouteValues["action"],
                traceId
            );

        }
    }
}
