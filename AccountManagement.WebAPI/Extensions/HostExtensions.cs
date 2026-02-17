using Serilog;

namespace AccountManagement.WebAPI.Extensions
{
    public static class HostExtensions
    {
            public static IHostBuilder ConfigureSerilog(this IHostBuilder hostBuilder)
            {
                hostBuilder.UseSerilog((context, services, configuration) =>
                {
                    configuration
                        .ReadFrom.Configuration(context.Configuration)
                        .ReadFrom.Services(services)
                        .Enrich.FromLogContext()
                        .WriteTo.Console()
                        .WriteTo.File(
                            path: "Logs/log-.txt", 
                            rollingInterval: RollingInterval.Day, 
                            shared: true, 
                            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
                });

                return hostBuilder;
            }
        }

}
