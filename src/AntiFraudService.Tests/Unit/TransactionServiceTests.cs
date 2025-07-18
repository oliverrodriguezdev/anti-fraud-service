using AntiFraudService.Application.DTOs;
using AntiFraudService.Application.Services;
using AntiFraudService.Domain.Entities;
using AntiFraudService.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AntiFraudService.Tests.Unit;

public class TransactionServiceTests
{
    private readonly DbContextOptions<AppDbContext> _options;

    public TransactionServiceTests()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task CreateTransactionAsync_WithValidData_ShouldCreateTransaction()
    {
        // Arrange
        using var context = new AppDbContext(_options);
        var service = new TransactionService(context, Mock.Of<Infrastructure.Messaging.KafkaEventPublisher>());
        var dto = new CreateTransactionDto
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = 1,
            Value = 100
        };

        // Act
        var result = await service.CreateTransactionAsync(dto);

        // Assert
        result.Should().NotBeEmpty();
        
        var transaction = await context.Transactions.FirstOrDefaultAsync(t => t.Id == result);
        transaction.Should().NotBeNull();
        transaction!.SourceAccountId.Should().Be(dto.SourceAccountId);
        transaction.TargetAccountId.Should().Be(dto.TargetAccountId);
        transaction.TransferTypeId.Should().Be(dto.TransferTypeId);
        transaction.Value.Should().Be(dto.Value);
        transaction.Status.Should().Be("pending");
    }

    [Fact]
    public async Task GetTransactionAsync_WithExistingId_ShouldReturnTransaction()
    {
        // Arrange
        using var context = new AppDbContext(_options);
        var service = new TransactionService(context, Mock.Of<Infrastructure.Messaging.KafkaEventPublisher>());
        
        var transaction = new Transaction
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = 1,
            Value = 100,
            Status = "approved"
        };
        
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetTransactionAsync(transaction.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(transaction.Id);
        result.Status.Should().Be("approved");
    }

    [Fact]
    public async Task GetTransactionAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        using var context = new AppDbContext(_options);
        var service = new TransactionService(context, Mock.Of<Infrastructure.Messaging.KafkaEventPublisher>());

        // Act
        var result = await service.GetTransactionAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }
} 