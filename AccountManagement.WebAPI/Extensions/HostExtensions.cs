using AccountManagement.WebAPI.Logging;
using Serilog;

namespace AccountManagement.WebAPI.Extensions
{
    public static class HostExtensions
    {
            public static IHostBuilder ConfigureSerilog(this IHostBuilder hostBuilder)
            {
                hostBuilder.UseSerilog((context, services, configuration) =>
                {
                    // Read custom path from config
                    var logFilePath = context.Configuration["LoggingOptions:LogFilePath"] 
                            ?? "Logs/api-log-.json"; // fallback if not set

                    configuration
                        .ReadFrom.Configuration(context.Configuration)
                        .ReadFrom.Services(services)
                        .Enrich.FromLogContext()
                        .Enrich.With<StackTraceEnricher>() // ✅ register your custom enricher
                        .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
                        .WriteTo.File(
                            new Serilog.Formatting.Json.JsonFormatter(), 
                            path: logFilePath,
                            rollingInterval: RollingInterval.Day,
                            fileSizeLimitBytes: 1 * 1024 * 1024 , // 1 MB
                            rollOnFileSizeLimit: true,
                            shared: true
                        );
                });

                return hostBuilder;
            }
        }

}
