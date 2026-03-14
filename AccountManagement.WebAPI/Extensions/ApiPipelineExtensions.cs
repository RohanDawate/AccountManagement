using AccountManagement.Application.Common;
using AccountManagement.Application.Common.Responses;

namespace AccountManagement.WebAPI.Extensions
{
    public static class ApiPipelineExtensions
    {
        public static IApplicationBuilder UseApiPipeline(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Custom middleware
            app.UseApiMiddleware();

            // Static files
            app.UseStaticFiles();

            // Endpoints
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapApiEndpoints(env); // now endpoints is IEndpointRouteBuilder
            });

            // Configure ApiResponseFactory
            using (var scope = app.ApplicationServices.CreateScope())
            {
                ITraceIdProvider provider = scope.ServiceProvider.GetRequiredService<ITraceIdProvider>();
                ApiResponseFactory.Configure(provider);
            }

            return app;
        }
    }
}
