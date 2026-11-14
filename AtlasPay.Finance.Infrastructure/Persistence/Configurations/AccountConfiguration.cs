using AtlasPay.Finance.Domain.Accounts.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasPay.Finance.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration
    : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        // This is not marked as a FK that's intentional
        // We shouldn't create a cross-microservice database foreign key.
        builder.Property(x => x.CustomerId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasConversion<string>()
            .HasMaxLength(3)
            .IsRequired();

        builder.HasIndex(x => x.CustomerId);
    }
}
