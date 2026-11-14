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
            builder.ToTable("ledger_accounts");

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

            builder.HasOne<Account>()
                .WithMany()
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.AccountId);
        }
    }
}
