using AccountManagement.Application.Common;
using AccountManagement.WebAPI.Extensions;


var builder = WebApplication.CreateBuilder(args);

// Host configuration
builder.Host.ConfigureSerilog();

// Service registration
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

// Middleware pipeline
app.UseApiMiddleware();

// Serve static files (favicon, css, js, etc.)
app.UseStaticFiles();

app.MapApiEndpoints(app.Environment);

// Configure ApiResponseFactory to use DI provider
using (var scope = app.Services.CreateScope())
{
    var provider = scope.ServiceProvider.GetRequiredService<ITraceIdProvider>();
    AccountManagement.Application.Common.Responses.ApiResponseFactory.Configure(provider);
}

app.Run();
