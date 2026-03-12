using AccountManagement.Application.Common;
using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AccountManagement.Infra.Interceptors
{
    public class LoggingInterceptor : IInterceptor
    {
        private readonly ILogger<LoggingInterceptor> _logger;
        private readonly ITraceIdProvider _traceIdProvider;

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false
        };

        public LoggingInterceptor(ILogger<LoggingInterceptor> logger, ITraceIdProvider traceIdProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _traceIdProvider = traceIdProvider ?? throw new ArgumentNullException(nameof(traceIdProvider));
        }

        public void Intercept(IInvocation invocation)
        {
            var traceId = _traceIdProvider.GetTraceId() ?? "<NoTraceId>";

            if (_logger.IsEnabled(LogLevel.Information))
            {
                var argsJson = JsonSerializer.Serialize(invocation.Arguments ?? Array.Empty<object>(), _jsonOptions);
                _logger.LogInformation("Entering {Class}.{Method} with args {Args}. TraceId={TraceId}",
                    invocation.TargetType?.Name ?? "<UnknownClass>",
                    invocation.Method?.Name ?? "<UnknownMethod>",
                    argsJson,
                    traceId);
            }

            try
            {
                invocation.Proceed();

                // Handle async methods
                if (invocation.Method?.ReturnType != null &&
                    typeof(Task).IsAssignableFrom(invocation.Method.ReturnType))
                {
                    var task = (Task)invocation.ReturnValue!;
                    if (invocation.Method.ReturnType.IsGenericType)
                    {
                        // Task<T>
                        var resultType = invocation.Method.ReturnType.GetGenericArguments()[0];
                        invocation.ReturnValue = InterceptGenericTask(task, resultType, invocation, traceId);
                    }
                    else
                    {
                        // Task (non-generic)
                        invocation.ReturnValue = InterceptTask(task, invocation, traceId);
                    }
                }
                else
                {
                    // Sync methods
                    LogReturnValue(invocation, traceId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in {Class}.{Method}. TraceId={TraceId}",
                    invocation.TargetType?.Name ?? "<UnknownClass>",
                    invocation.Method?.Name ?? "<UnknownMethod>",
                    traceId);
                throw;
            }
        }

        private async Task InterceptTask(Task task, IInvocation invocation, string traceId)
        {
            try
            {
                await task;
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Exiting {Class}.{Method} (async void). TraceId={TraceId}",
                        invocation.TargetType?.Name ?? "<UnknownClass>",
                        invocation.Method?.Name ?? "<UnknownMethod>",
                        traceId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in {Class}.{Method} (async void). TraceId={TraceId}",
                    invocation.TargetType?.Name ?? "<UnknownClass>",
                    invocation.Method?.Name ?? "<UnknownMethod>",
                    traceId);
                throw;
            }
        }

        private object InterceptGenericTask(Task task, Type resultType, IInvocation invocation, string traceId)
        {
            var method = typeof(LoggingInterceptor)
                .GetMethod(nameof(InterceptGenericTaskCore), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .MakeGenericMethod(resultType);

            return method.Invoke(
                this,
                new object[]
                {
                    task ?? throw new ArgumentNullException(nameof(task)),
                    invocation ?? throw new ArgumentNullException(nameof(invocation)),
                    traceId ?? "<NoTraceId>"
                })!;

        }

        private async Task<T> InterceptGenericTaskCore<T>(Task task, IInvocation invocation, string traceId)
        {
            try
            {
                var result = await (Task<T>)task;
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    var returnValue = result == null ? "null" : JsonSerializer.Serialize(result, _jsonOptions);
                    _logger.LogInformation("Exiting {Class}.{Method} with async return {ReturnValue}. TraceId={TraceId}",
                        invocation.TargetType?.Name ?? "<UnknownClass>",
                        invocation.Method?.Name ?? "<UnknownMethod>",
                        returnValue,
                        traceId);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in {Class}.{Method} (async). TraceId={TraceId}",
                    invocation.TargetType?.Name ?? "<UnknownClass>",
                    invocation.Method?.Name ?? "<UnknownMethod>",
                    traceId);
                throw;
            }
        }

        private void LogReturnValue(IInvocation invocation, string traceId)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                if (invocation.Method?.ReturnType != typeof(void))
                {
                    var returnValue = invocation.ReturnValue == null
                        ? "null"
                        : JsonSerializer.Serialize(invocation.ReturnValue, _jsonOptions);

                    _logger.LogInformation("Exiting {Class}.{Method} with return value {ReturnValue}. TraceId={TraceId}",
                        invocation.TargetType?.Name ?? "<UnknownClass>",
                        invocation.Method?.Name ?? "<UnknownMethod>",
                        returnValue,
                        traceId);
                }
                else
                {
                    _logger.LogInformation("Exiting {Class}.{Method} (void). TraceId={TraceId}",
                        invocation.TargetType?.Name ?? "<UnknownClass>",
                        invocation.Method?.Name ?? "<UnknownMethod>",
                        traceId);
                }
            }
        }
    }
}