using AtlasPay.Finance.Domain.Ledger.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasPay.Finance.Infrastructure.Persistence.Configurations;

public sealed class AccountBalanceConfiguration
    : IEntityTypeConfiguration<AccountBalance>
{
    public void Configure(EntityTypeBuilder<AccountBalance> builder)
    {
        builder.ToTable("account_balances");

        builder.HasKey(x => x.LedgerAccountId);

        builder.Property(x => x.LedgerAccountId)
            .ValueGeneratedNever();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.ComplexProperty(x => x.Balance, money =>
        {
            money.Property(x => x.Amount)
                .HasColumnName("balance")
                .HasPrecision(18, 2);

            money.Property(x => x.Currency)
                .HasColumnName("currency")
                .HasConversion<string>()
                .HasMaxLength(3);
        });

        builder.HasOne<LedgerAccount>()
            .WithOne()
            .HasForeignKey<AccountBalance>(
                x => x.LedgerAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}