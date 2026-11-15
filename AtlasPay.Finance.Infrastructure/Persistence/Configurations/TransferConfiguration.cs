using AtlasPay.Finance.Domain.Accounts.Entities;
using AtlasPay.Finance.Domain.Transfers.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasPay.Finance.Infrastructure.Persistence.Configurations;

public sealed class TransferConfiguration
    : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.ToTable("transfers", table =>
        {
            table.HasCheckConstraint(
                "ck_transfers_amount_positive",
                "amount > 0");

            table.HasCheckConstraint(
                "ck_transfers_different_accounts",
                "\"FromAccountId\" <> \"ToAccountId\"");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.FromAccountId)
            .IsRequired();

        builder.Property(x => x.ToAccountId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CompletedAt)
            .IsRequired(false);

        builder.Property(x => x.RequestId)
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

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(x => x.FromAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(x => x.ToAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.FromAccountId);

        builder.HasIndex(x => x.ToAccountId);

        builder.HasIndex(x => x.CreatedAt);

        builder.HasIndex(x => x.RequestId).IsUnique();
    }
}