using AccountManagement.WebAPI.Middleware;

namespace AccountManagement.WebAPI.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseApiMiddlewares(this IApplicationBuilder app)
        {
            // 1️. TraceId enrichment — push TraceId once
            app.UseMiddleware<TraceIdMiddleware>();

            // 2. Request/Response logging — logs structured request/response
            app.UseMiddleware<RequestResponseLoggingMiddleware>();

            // 3. Exception handling — centralized, catches everything downstream
            app.UseMiddleware<ExceptionMiddleware>(); 

            // 4. Envelope every response uniformly
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
