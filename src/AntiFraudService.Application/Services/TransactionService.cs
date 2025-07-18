using AntiFraudService.Application.DTOs;
using AntiFraudService.Application.Interfaces;
using AntiFraudService.Domain.Entities;
using AntiFraudService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AntiFraudService.Infrastructure.Messaging;
using Microsoft.Extensions.Logging;

namespace AntiFraudService.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly AppDbContext _context;
    private readonly KafkaEventPublisher _publisher;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(AppDbContext context, KafkaEventPublisher publisher, ILogger<TransactionService> logger)
    {
        _context = context;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<Guid> CreateTransactionAsync(CreateTransactionDto dto)
    {
        var transaction = new Transaction
        {
            SourceAccountId = dto.SourceAccountId,
            TargetAccountId = dto.TargetAccountId,
            TransferTypeId = dto.TransferTypeId,
            Value = dto.Value,
            Status = "pending"
        };

        _logger.LogInformation("Creating transaction with value: {Value}", dto.Value);
        
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Transaction saved to database with ID: {Id}", transaction.Id);

        // Publicar en Kafka
        _logger.LogInformation("Publishing transaction to Kafka: {TransactionId}", transaction.Id);
        await _publisher.PublishAsync("transactions", transaction);

        _logger.LogInformation("Transaction published to Kafka successfully");

        return transaction.Id;
    }

    public async Task<Transaction?> GetTransactionAsync(Guid id)
    {
        return await _context.Transactions.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
    {
        return await _context.Transactions.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }
}
