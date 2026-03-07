using AccountManagement.Application.Interfaces;
using AccountManagement.Application.Services;
using AccountManagement.Infra.Interceptors;
using AccountManagement.Infra.Repositories;
using AccountManagement.WebAPI.Extensions;
using Castle.DynamicProxy;

var builder = WebApplication.CreateBuilder(args);

// Host configuration
builder.Host.ConfigureSerilog();

// Service registration
builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddInterceptedServices(); // registers services + interceptors

//// Register interceptor
//builder.Services.AddScoped<LoggingInterceptor>();

//builder.Services.AddScoped<IOrderService, OrderService>();
//builder.Services.AddScoped<IOrderRepository, OrderRepository>();

//var proxyGenerator = new ProxyGenerator();

//builder.Services.Decorate<IOrderService>((inner, provider) =>
//{
//    var interceptor = provider.GetRequiredService<LoggingInterceptor>();
//    return proxyGenerator.CreateInterfaceProxyWithTarget(inner, interceptor);
//});

//builder.Services.Decorate<IOrderRepository>((inner, provider) =>
//{
//    var interceptor = provider.GetRequiredService<LoggingInterceptor>();
//    return proxyGenerator.CreateInterfaceProxyWithTarget(inner, interceptor);
//});

var app = builder.Build();

// Single unified pipeline call
app.UseApiPipeline(app.Environment);

app.Run();

