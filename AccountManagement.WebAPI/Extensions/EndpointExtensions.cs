namespace AccountManagement.WebAPI.Extensions
{
    public static class EndpointExtensions
    {
        public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder endpoints,
            IWebHostEnvironment env)
        {
            // Root endpoint for friendly startup message
            endpoints.MapGet("/", () => Results.Ok(new
            {
                message = "Account Management API is running...",
                environment = env.EnvironmentName
            }));

            // OpenAPI only in Development
            if (env.IsDevelopment())
            {
                endpoints.MapOpenApi();
            }

            // Controllers
            endpoints.MapControllers();

            return endpoints;
        }
    }
}
