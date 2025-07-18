using AntiFraudService.Domain.Entities;
using AntiFraudService.Infrastructure.Persistence;
using AntiFraudService.Worker;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.Kafka;
using Testcontainers.PostgreSql;
using Xunit;

namespace AntiFraudService.Tests.Integration;

[Trait("Category", "Integration")]
public class WorkerIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private readonly KafkaContainer _kafkaContainer;
    private readonly IHost _host;

    public WorkerIntegrationTests()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithDatabase("antifraud_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        _kafkaContainer = new KafkaBuilder()
            .WithName("test-kafka")
            .Build();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"ConnectionStrings:DefaultConnection", _postgresContainer.GetConnectionString()},
                {"Kafka:BootstrapServers", _kafkaContainer.GetBootstrapAddress()},
                {"Kafka:ConsumerGroupId", "test-consumer-group"},
                {"Kafka:Topics:Transactions", "transactions"},
                {"Kafka:Topics:TransactionStatus", "transactions-status"}
            })
            .Build();

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(_postgresContainer.GetConnectionString()));
                services.AddInfrastructure(configuration);
                services.AddHostedService<Worker>();
            })
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        await _kafkaContainer.StartAsync();
        
        // Aplicar migraciones
        using var scope = _host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _host.StopAsync();
        await _postgresContainer.DisposeAsync();
        await _kafkaContainer.DisposeAsync();
    }

    [Fact]
    public async Task Worker_ShouldProcessTransaction_AndUpdateStatus()
    {
        // Arrange
        using var scope = _host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var transaction = new Transaction
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = 1,
            Value = 100,
            Status = "pending"
        };
        
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        // Act - Simular publicación en Kafka (en un test real usarías el producer)
        await _host.StartAsync();
        
        // Esperar un tiempo para que el worker procese
        await Task.Delay(5000);
        
        await _host.StopAsync();

        // Assert
        var updatedTransaction = await context.Transactions.FirstOrDefaultAsync(t => t.Id == transaction.Id);
        updatedTransaction.Should().NotBeNull();
        updatedTransaction!.Status.Should().Be("approved");
    }

    [Fact]
    public async Task Worker_ShouldRejectTransaction_WithHighAmount()
    {
        // Arrange
        using var scope = _host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var transaction = new Transaction
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = 1,
            Value = 2500, // Mayor a 2000
            Status = "pending"
        };
        
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        // Act
        await _host.StartAsync();
        await Task.Delay(5000);
        await _host.StopAsync();

        // Assert
        var updatedTransaction = await context.Transactions.FirstOrDefaultAsync(t => t.Id == transaction.Id);
        updatedTransaction.Should().NotBeNull();
        updatedTransaction!.Status.Should().Be("rejected");
    }
} 