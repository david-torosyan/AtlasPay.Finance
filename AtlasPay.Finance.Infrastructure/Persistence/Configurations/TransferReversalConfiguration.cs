
using AtlasPay.Finance.Domain.Transfers.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasPay.Finance.Infrastructure.Persistence.Configurations;

public sealed class TransferReversalConfiguration
    : IEntityTypeConfiguration<TransferReversal>
{
    public void Configure(
        EntityTypeBuilder<TransferReversal> builder)
    {
        builder.ToTable("transfer_reversals");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.RequestId)
            .IsRequired();

        builder.Property(x => x.OriginalTransferId)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne<Transfer>()
            .WithOne()
            .HasForeignKey<TransferReversal>(
                x => x.OriginalTransferId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.RequestId)
            .IsUnique();
    }
}
