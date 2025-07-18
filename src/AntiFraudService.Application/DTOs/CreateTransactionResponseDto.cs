namespace AntiFraudService.Application.DTOs;
 
public class CreateTransactionResponseDto
{
    public Guid TransactionExternalId { get; set; }
    public DateTime CreatedAt { get; set; }
} 