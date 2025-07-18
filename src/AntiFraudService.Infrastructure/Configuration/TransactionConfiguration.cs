using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AntiFraudService.Domain.Entities;

namespace AntiFraudService.Infrastructure.Persistence.Configuration;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.SourceAccountId)
            .IsRequired();
        builder.Property(t => t.TargetAccountId)
            .IsRequired();
        builder.Property(t => t.TransferTypeId)
            .IsRequired();
        builder.Property(t => t.Value)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.Status)
            .IsRequired()
            .HasMaxLength(20);
    }
}
