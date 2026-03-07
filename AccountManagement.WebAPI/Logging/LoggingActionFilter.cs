using Microsoft.AspNetCore.Mvc.Filters;

namespace AccountManagement.WebAPI.Logging
{
    public class LoggingActionFilter : IActionFilter
    {
        private readonly ILogger<LoggingActionFilter> _logger;

        public LoggingActionFilter(ILogger<LoggingActionFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _logger.LogInformation("Executing {ActionName}", context.ActionDescriptor.DisplayName);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _logger.LogInformation("Executed {ActionName}", context.ActionDescriptor.DisplayName);
        }
    }
}
