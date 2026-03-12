using AccountManagement.Application.Common;
using AccountManagement.Application.Interfaces;
using AccountManagement.Application.Services;
using AccountManagement.Application.Validators;
using AccountManagement.Infra.Interceptors;
using AccountManagement.Infra.Repositories;
using AccountManagement.WebAPI.Logging;
using Castle.DynamicProxy;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace AccountManagement.WebAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            // Register providers
            services.AddHttpContextAccessor();
            services.AddScoped<ITraceIdProvider, TraceIdProvider>();

            // Controllers
            services
                .AddControllers(options =>
                {
                    options.Filters.Add<LoggingActionFilter>();
                })
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
            // Register interceptors
            services.AddScoped<LoggingInterceptor>();
            // services.AddSingleton<ValidationInterceptor>();
            // services.AddSingleton<RetryInterceptor>();

            // Register ProxyGenerator once
            services.AddSingleton<ProxyGenerator>();

            // Register OrderService and OrderRepository
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderRepository, OrderRepository>();

            // Decorate OrderService
            services.Decorate<IOrderService>((inner, provider) =>
            {
                var proxyGenerator = provider.GetRequiredService<ProxyGenerator>();
                var interceptor = provider.GetRequiredService<LoggingInterceptor>();
                return proxyGenerator.CreateInterfaceProxyWithTarget(inner, interceptor);
            });

            // Decorate OrderRepository
            services.Decorate<IOrderRepository>((inner, provider) =>
            {
                var proxyGenerator = provider.GetRequiredService<ProxyGenerator>();
                var interceptor = provider.GetRequiredService<LoggingInterceptor>();
                return proxyGenerator.CreateInterfaceProxyWithTarget(inner, interceptor);
            });

            return services;
        }

    }
}
