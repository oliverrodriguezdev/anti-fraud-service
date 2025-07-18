using Microsoft.Extensions.Configuration;
using AntiFraudService.Domain.Entities;
using AntiFraudService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using AntiFraudService.Infrastructure.Messaging;

namespace AntiFraudService.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly KafkaEventPublisher _publisher;

    public Worker(ILogger<Worker> logger, IConfiguration configuration, KafkaEventPublisher publisher)
    {
        _logger = logger;
        _configuration = configuration;
        _publisher = publisher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var consumerLogger = loggerFactory.CreateLogger<KafkaEventConsumer>();
        var consumer = new KafkaEventConsumer(_configuration, "transactions", consumerLogger);

        await Task.Run(() =>
        {
            consumer.Consume(async message =>
            {
                try
                {
                    var transaction = JsonSerializer.Deserialize<Transaction>(message);

                    if (transaction == null)
                    {
                        _logger.LogWarning("Mensaje inválido recibido de Kafka.");
                        return;
                    }

                    // Validación anti-fraude
                    string newStatus = "approved";
                    if (transaction.Value > 2000)
                    {
                        newStatus = "rejected";
                    }
                    else
                    {
                        // Acumulado diario
                        using var db = new AppDbContext(
                            new DbContextOptionsBuilder<AppDbContext>()
                                .UseNpgsql(_configuration.GetConnectionString("DefaultConnection"))
                                .Options);

                        var dailyTotal = db.Transactions
                            .Where(t => t.SourceAccountId == transaction.SourceAccountId && t.CreatedAt.Date == transaction.CreatedAt.Date)
                            .Sum(t => t.Value);

                        if (dailyTotal + transaction.Value > 20000)
                            newStatus = "rejected";
                    }

                    // Actualizar estado en la base de datos
                    using (var db = new AppDbContext(
                        new DbContextOptionsBuilder<AppDbContext>()
                            .UseNpgsql(_configuration.GetConnectionString("DefaultConnection"))
                            .Options))
                    {
                        var tx = db.Transactions.FirstOrDefault(t => t.Id == transaction.Id);
                        if (tx != null)
                        {
                            tx.Status = newStatus;
                            await db.SaveChangesAsync();
                            _logger.LogInformation("Transacción {id} actualizada a {status}", tx.Id, tx.Status);

                            // Publicar el resultado en el topic "transactions-status"
                            await _publisher.PublishAsync("transactions-status", new
                            {
                                TransactionId = tx.Id,
                                Status = tx.Status,
                                UpdatedAt = DateTime.UtcNow
                            }, stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando mensaje de Kafka");
                }
            }, stoppingToken);
        }, stoppingToken);
    }
}
