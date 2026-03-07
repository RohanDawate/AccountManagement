using AccountManagement.Application.Common;
using AccountManagement.Application.Interfaces;
using AccountManagement.Application.Services;
using AccountManagement.Application.Validators;
using AccountManagement.Infra.Interceptors;
using AccountManagement.Infra.Repositories;
using Castle.DynamicProxy;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Scrutor;
using Castle.DynamicProxy;


namespace AccountManagement.WebAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration config)
        {
            // Register providers
            services.AddHttpContextAccessor();
            services.AddScoped<ITraceIdProvider, TraceIdProvider>();

            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<OrderService>();

            // Controllers
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                });


            // OpenAPI + Endpoints Explorer
            services.AddOpenApi();
            services.AddEndpointsApiExplorer();

            // Register FluentValidation
            services.AddFluentValidationAutoValidation()
                            .AddFluentValidationClientsideAdapters();


            // FluentValidation
            services.AddValidatorsFromAssemblyContaining<ProductValidator>();

            // Disable automatic 400 responses from [ApiController]
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressMapClientErrors = true;
                options.SuppressModelStateInvalidFilter = true;
            });

            return services;

        }


        public static IServiceCollection AddInterceptedServices(this IServiceCollection services)
        {
            var proxyGenerator = new ProxyGenerator();

            // Register interceptors
            services.AddSingleton<LoggingInterceptor>();
            //services.AddSingleton<ValidationInterceptor>();
            //services.AddSingleton<RetryInterceptor>();

            // Scan for services/repositories
            services.Scan(scan => scan
                .FromAssemblies(typeof(OrderService).Assembly, typeof(OrderRepository).Assembly)
                .AddClasses(classes => classes.Where(t =>
                    t.Name.EndsWith("Service") || t.Name.EndsWith("Repository")))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // Decorate with Castle proxies
            foreach (var service in services.Where(s => s.ServiceType.IsInterface))
            {
                services.Decorate(service.ServiceType, (inner, provider) =>
                {
                    var interceptors = new IInterceptor[]
                    {
                    provider.GetRequiredService<LoggingInterceptor>(),
                    //provider.GetRequiredService<ValidationInterceptor>(),
                    //provider.GetRequiredService<RetryInterceptor>()
                    };

                    return proxyGenerator.CreateInterfaceProxyWithTarget(service.ServiceType, inner, interceptors);
                });
            }

            return services;
        }

    }
}
