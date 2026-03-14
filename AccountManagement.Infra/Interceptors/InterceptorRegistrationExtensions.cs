using AccountManagement.Application.Common;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;

namespace AccountManagement.Infra.Interceptors
{
    public static class InterceptorRegistrationExtensions
    {
        public static IServiceCollection AddSolutionWideInterceptors(this IServiceCollection services)
        {
            // Register dependencies for LoggingInterceptor
            services.AddSingleton<ProxyGenerator>();
            services.AddScoped<ITraceIdProvider, TraceIdProvider>();
            services.AddTransient<LoggingInterceptor>();

            // Decorate all interfaces with LoggingInterceptor
            foreach (var descriptor in services.ToList())
            {
                var type = descriptor.ServiceType;
                if (type.IsInterface &&
                    descriptor.ImplementationType != null &&
                    type.Namespace != null &&
                    type.Namespace.StartsWith("AccountManagement")) // app’s namespace, avoid Microsoft & System types
                {
                    services.Decorate(type, (inner, provider) =>
                    {
                        var proxyGenerator = provider.GetRequiredService<ProxyGenerator>();
                        var interceptor = provider.GetRequiredService<LoggingInterceptor>();

                        return proxyGenerator.CreateInterfaceProxyWithTargetInterface(
                            type,
                            inner,
                            interceptor);
                    });
                }
            }

            return services;
        }
    }
}
