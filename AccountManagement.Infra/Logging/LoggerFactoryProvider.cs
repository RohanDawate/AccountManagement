using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace AccountManagement.Infra.Logging
{
    public static class LoggerFactoryProvider
    {
        public static ILoggerFactory Create(string logFilePath)
        {
            var serilogLogger = new LoggerConfiguration()
                .WriteTo.File(logFilePath,
                              rollingInterval: RollingInterval.Day,
                              retainedFileCountLimit: 10)
                .CreateLogger();

            return new SerilogLoggerFactory(serilogLogger, dispose: true);
        }
    }
}
