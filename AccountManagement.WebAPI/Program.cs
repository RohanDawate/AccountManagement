using AccountManagement.WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Host configuration for Serilog
builder.Host.ConfigureSerilog();

// Service registration
builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddInterceptedServices(); // registers services + interceptors

var app = builder.Build();

// Single unified pipeline call
app.UseApiPipeline(app.Environment);

app.Run();

