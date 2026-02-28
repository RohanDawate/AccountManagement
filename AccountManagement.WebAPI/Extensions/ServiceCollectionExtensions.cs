using AccountManagement.Application.Common;
using AccountManagement.Application.Validators;
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
