using AtlasPay.Finance.Domain.Outbox.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasPay.Finance.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration
    : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure
        (EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.EventType)
           .HasColumnName("event_type")
           .HasMaxLength(200)
           .IsRequired();

        builder.Property(x => x.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
           .HasColumnName("created_at")
           .IsRequired();

        builder.Property(x => x.PublishedAt)
            .HasColumnName("published_at");

        builder.Property(x => x.Attempts)
           .HasColumnName("attempts")
           .HasDefaultValue(0)
           .IsRequired();

        builder.Property(x => x.NextAttemptAt)
            .HasColumnName("next_attempt_at")
            .IsRequired();

        builder.Property(x => x.LockedUntil)
            .HasColumnName("locked_until");

        builder.Property(x => x.LockedBy)
            .HasColumnName("locked_by")
            .HasMaxLength(200);

        builder.Property(x => x.LastError)
            .HasColumnName("last_error")
            .HasMaxLength(2000);

        builder.HasIndex(x => new
        {
            x.NextAttemptAt,
            x.CreatedAt
        })
        .HasDatabaseName("ix_outbox_messages_pending")
        .HasFilter("published_at IS NULL");
    }
}
