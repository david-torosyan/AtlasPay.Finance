using AtlasPay.Finance.Domain.Ledger.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasPay.Finance.Infrastructure.Persistence.Configurations;

public sealed class LedgerPostingConfiguration
    : IEntityTypeConfiguration<LedgerPosting>
{
    public void Configure(
        EntityTypeBuilder<LedgerPosting> builder)
    {
        builder.ToTable("ledger_postings", table =>
        {
            table.HasCheckConstraint(
                "ck_ledger_postings_amount_positive",
                "amount > 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.LedgerTransactionId)
            .IsRequired();

        builder.Property(x => x.LedgerAccountId)
            .IsRequired();

        builder.Property(x => x.Direction)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.ComplexProperty(x => x.Amount, money =>
        {
            money.Property(x => x.Amount)
                .HasColumnName("amount")
                .HasPrecision(18, 2);

            money.Property(x => x.Currency)
                .HasColumnName("currency")
                .HasConversion<string>()
                .HasMaxLength(3);
        });

        builder.HasOne<LedgerAccount>()
            .WithMany()
            .HasForeignKey(x => x.LedgerAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.LedgerTransactionId);

        builder.HasIndex(x => x.LedgerAccountId);
    }
}
