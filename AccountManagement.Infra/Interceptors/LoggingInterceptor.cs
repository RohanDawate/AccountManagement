using AccountManagement.Application.Common;
using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;

namespace AccountManagement.Infra.Interceptors
{
    public class LoggingInterceptor : IInterceptor
    {
        private readonly ILogger<LoggingInterceptor> _logger;
        private readonly ITraceIdProvider _traceIdProvider;

        public LoggingInterceptor(ILogger<LoggingInterceptor> logger, ITraceIdProvider traceIdProvider)
        {
            _logger = logger;
            _traceIdProvider = traceIdProvider;
        }


        public void Intercept(IInvocation invocation)
        {
            var traceId = _traceIdProvider.GetTraceId();

            _logger.LogInformation("Entering {Class}.{Method} TraceId={TraceId}",
                invocation.TargetType.Name, invocation.Method.Name, traceId);

            try
            {
                invocation.Proceed();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in {Class}.{Method} TraceId={TraceId}",
                    invocation.TargetType.Name, invocation.Method.Name, traceId);
                throw;
            }

            _logger.LogInformation("Exiting {Class}.{Method} TraceId={TraceId}",
                invocation.TargetType.Name, invocation.Method.Name, traceId);

        }

    }
}
