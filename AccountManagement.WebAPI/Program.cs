using AccountManagement.WebAPI.Extensions;


var builder = WebApplication.CreateBuilder(args);

// Host configuration
builder.Host.ConfigureSerilog();

// Service registration
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

// Middleware pipeline
app.UseApiMiddlewares(); 
app.MapApiEndpoints(app.Environment);

app.Run();
