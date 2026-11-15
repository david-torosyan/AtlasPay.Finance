using AtlasPay.Finance.Domain.Accounts.Entities;
using AtlasPay.Finance.Domain.Ledger.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasPay.Finance.Infrastructure.Persistence.Configurations
{
    public sealed class LedgerAccountConfiguration
        : IEntityTypeConfiguration<LedgerAccount>
    {
        public void Configure(EntityTypeBuilder<LedgerAccount> builder)
        {
            builder.ToTable("ledger_accounts", table =>
            {
                table.HasCheckConstraint(
                    "ck_ledger_accounts_owner_or_code",
                    """
                    ("AccountId" IS NOT NULL AND "Code" IS NULL)
                    OR
                    ("AccountId" IS NULL AND "Code" IS NOT NULL)
                    """);
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.AccountId)
                .IsRequired(false);

            builder.Property(x => x.Type)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.Currency)
                .HasConversion<string>()
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(x => x.Code)
                .HasMaxLength(64)
                .IsRequired(false);

            builder.HasOne<Account>()
                .WithMany()
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.AccountId);

            builder.HasIndex(x => x.Code)
                .IsUnique()
                .HasFilter("\"Code\" IS NOT NULL");
        }
    }
}
