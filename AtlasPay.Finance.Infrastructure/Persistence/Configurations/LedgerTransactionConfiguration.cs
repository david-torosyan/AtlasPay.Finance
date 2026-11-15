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

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.ReversesTransactionId)
            .IsRequired(false);

        builder.HasOne<LedgerTransaction>()
            .WithOne()
            .HasForeignKey<LedgerTransaction>(
                x => x.ReversesTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ReferenceId)
            .IsUnique();

        builder.HasMany(x => x.Postings)
            .WithOne()
            .HasForeignKey(x => x.LedgerTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.Postings)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.ReversesTransactionId)
            .IsUnique()
            .HasFilter("\"ReversesTransactionId\" IS NOT NULL");
    }
}