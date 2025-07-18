using AntiFraudService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AntiFraudService.Tests;

public abstract class TestBase
{
    protected readonly IServiceProvider ServiceProvider;
    protected readonly AppDbContext DbContext;

    protected TestBase()
    {
        var services = new ServiceCollection();
        
        // Configurar base de datos en memoria para tests
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        
        // Configurar configuración mínima
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"Kafka:BootstrapServers", "localhost:9092"},
                {"Kafka:ConsumerGroupId", "test-group"},
                {"Kafka:Topics:Transactions", "transactions"},
                {"Kafka:Topics:TransactionStatus", "transactions-status"}
            })
            .Build();
        
        services.AddSingleton<IConfiguration>(configuration);
        services.AddInfrastructure(configuration);
        
        ServiceProvider = services.BuildServiceProvider();
        DbContext = ServiceProvider.GetRequiredService<AppDbContext>();
    }

    protected async Task<AppDbContext> GetDbContextAsync()
    {
        await DbContext.Database.EnsureCreatedAsync();
        return DbContext;
    }

    protected async Task CleanupAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
    }
} 