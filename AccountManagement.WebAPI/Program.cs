using AccountManagement.WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Host configuration
builder.Host.ConfigureSerilog();

// Service registration
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

// Single unified pipeline call
app.UseApiPipeline(app.Environment);

app.Run();

