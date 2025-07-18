using AntiFraudService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using AntiFraudService.Application.Interfaces;
using AntiFraudService.Application.Services;
using AntiFraudService.API.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/api-.txt", 
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Validar variables de entorno críticas
var environment = builder.Environment.EnvironmentName;
Log.Information("Starting API in {Environment} environment", environment);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configurar Swagger solo en Development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
        { 
            Title = "Anti-Fraud Service API", 
            Version = "v1",
            Description = $"API para validación anti-fraude de transacciones - Environment: {environment}"
        });
    });
}

builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Anti-Fraud Service API v1");
        c.RoutePrefix = string.Empty; // Para que Swagger esté en la raíz
    });
}

// Configuraciones específicas por ambiente
var apiSettings = builder.Configuration.GetSection("ApiSettings").Get<ApiSettings>();
if (apiSettings?.RequireHttps == true)
{
    app.UseHttpsRedirection();
}

if (apiSettings?.CorsEnabled == true)
{
    app.UseCors();
}

app.UseAuthorization();
app.MapControllers();

Log.Information("API configured for environment: {Environment}", environment);

try
{
    Log.Information("Starting API...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
