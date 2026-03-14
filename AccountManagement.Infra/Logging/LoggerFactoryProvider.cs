using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace AccountManagement.Infra.Logging
{
    public static class LoggerFactoryProvider
    {
        private static ILoggerFactory? _loggerFactory;

        public static ILoggerFactory GetLoggerFactory(string contextName = "")
        {
            if (_loggerFactory != null) return _loggerFactory;

            var logFileName = LoggerFileNameResolver.Resolve(contextName);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(logFileName, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            _loggerFactory = new SerilogLoggerFactory(Log.Logger, dispose: true);

            return _loggerFactory;
        }

        public static ILogger<T> CreateLogger<T>(string contextName = "")
        {
            return GetLoggerFactory(contextName).CreateLogger<T>();
        }
    }
}
