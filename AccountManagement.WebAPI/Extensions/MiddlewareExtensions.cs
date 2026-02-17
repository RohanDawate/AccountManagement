using AccountManagement.WebAPI.Middleware;

namespace AccountManagement.WebAPI.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseApiMiddlewares(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseMiddleware<RequestResponseLoggingMiddleware>();
            //app.UseMiddleware<ApiResponseMiddleware>();
            app.UseMiddleware<UnifiedResponseMiddleware>();

            app.UseHttpsRedirection();
            app.UseAuthorization();
           
            return app;
        }

        public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder endpoints,
            IWebHostEnvironment env) 
        {
            // Root endpoint for friendly startup message
            endpoints.MapGet("/", () => Results.Ok(new 
            { 
                message = "AccountManagement API is running...", 
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
