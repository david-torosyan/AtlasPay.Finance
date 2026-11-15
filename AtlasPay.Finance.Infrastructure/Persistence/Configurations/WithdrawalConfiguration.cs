using AtlasPay.Finance.Domain.Accounts.Entities;
using AtlasPay.Finance.Domain.Deposits.Entities;
using AtlasPay.Finance.Domain.Withdrawals.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasPay.Finance.Infrastructure.Persistence.Configurations;

public sealed class WithdrawalConfiguration
    : IEntityTypeConfiguration<Withdrawal>
{
    public void Configure(EntityTypeBuilder<Withdrawal> builder)
    {
        builder.ToTable("withdrawals", table =>
        {
            table.HasCheckConstraint(
                "ck_deposits_amount_positive",
                "amount > 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.RequestId)
            .IsRequired();

        builder.Property(x => x.AccountId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
           .IsRequired();

        builder.Property(x => x.CompletedAt)
            .IsRequired(false);

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

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.AccountId,
            x.CreatedAt
        });
    }
}
