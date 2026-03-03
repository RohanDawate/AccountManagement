using AccountManagement.Application.Common;
using AccountManagement.Application.Interfaces;
using AccountManagement.Application.Services;
using AccountManagement.Application.Validators;
using AccountManagement.Infra.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;

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
    }
}
