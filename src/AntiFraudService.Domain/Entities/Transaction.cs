namespace AntiFraudService.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid(); // TransactionExternalId
    public Guid SourceAccountId { get; set; }
    public Guid TargetAccountId { get; set; }
    public int TransferTypeId { get; set; }
    public decimal Value { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "pending"; // Valores: pending, approved, rejected
}