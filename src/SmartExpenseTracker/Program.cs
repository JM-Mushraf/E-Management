using System.Reflection;
using Microsoft.OpenApi.Models;
using SmartExpenseTracker.Data.Abstractions;
using SmartExpenseTracker.Data.Implementations;
using SmartExpenseTracker.Middleware;
using SmartExpenseTracker.Service.Abstractions;
using SmartExpenseTracker.Service.Implementations;
using SmartExpenseTracker.Store.Abstractions;
using SmartExpenseTracker.Store.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Smart Expense Tracker API",
        Version = "v1",
        Description = "Clean, production-quality REST API for Smart Expense Tracker (Diligent Software Engineering Apprenticeship 2026)",
        Contact = new OpenApiContact
        {
            Name = "Smart Expense Tracker Team"
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Dependency Injection Registrations
builder.Services.AddSingleton<IJsonFileProvider, JsonFileProvider>();
builder.Services.AddScoped<IExpenseStore, ExpenseStore>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();

var app = builder.Build();

// Global Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Expense Tracker API v1");
        options.RoutePrefix = string.Empty; // Set Swagger UI at application root (/)
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
