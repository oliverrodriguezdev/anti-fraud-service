using AntiFraudService.Application.DTOs;
using AntiFraudService.Domain.Entities;

namespace AntiFraudService.Application.Interfaces;

public interface ITransactionService
{
    Task<Guid> CreateTransactionAsync(CreateTransactionDto dto);
    Task<Transaction?> GetTransactionAsync(Guid id);
    Task<IEnumerable<Transaction>> GetAllTransactionsAsync();
}
