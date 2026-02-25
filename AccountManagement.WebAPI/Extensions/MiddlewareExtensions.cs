using AccountManagement.WebAPI.Middleware;

namespace AccountManagement.WebAPI.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseApiMiddleware(this IApplicationBuilder app)
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

    }
}
