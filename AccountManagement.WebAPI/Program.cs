using AccountManagement.Application.Validators;
using AccountManagement.WebAPI.Middleware;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

// Register FluentValidation validators
builder.Services.AddValidatorsFromAssemblyContaining<ProductValidator>();

// Disable automatic 400 responses from [ApiController]
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>(); 
//app.UseMiddleware<ApiResponseMiddleware>();
app.UseMiddleware<UnifiedResponseMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
