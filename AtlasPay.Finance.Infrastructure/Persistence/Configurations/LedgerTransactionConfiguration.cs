using AtlasPay.Finance.Domain.Ledger.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasPay.Finance.Infrastructure.Persistence.Configurations;

public sealed class LedgerTransactionConfiguration
    : IEntityTypeConfiguration<LedgerTransaction>
{
    public void Configure(
        EntityTypeBuilder<LedgerTransaction> builder)
    {
        builder.ToTable("ledger_transactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.ReferenceId)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsPosted)
            .IsRequired();

        builder.HasIndex(x => x.ReferenceId)
            .IsUnique();

        builder.HasMany(x => x.Postings)
            .WithOne()
            .HasForeignKey(x => x.LedgerTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.Postings)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}