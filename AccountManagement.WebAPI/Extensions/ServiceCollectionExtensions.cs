using AccountManagement.Application.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace AccountManagement.WebAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration config)
        {
            // Controllers
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                });


            // OpenAPI + Endpoints Explorer
            services.AddOpenApi();
            services.AddEndpointsApiExplorer();

            // FluentValidation
            services.AddValidatorsFromAssemblyContaining<ProductValidator>();

            // Disable automatic 400 responses from [ApiController]
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            return services;

        }
    }
}
