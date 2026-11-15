
using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Domain.Outbox.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Finance.Infrastructure.Persistence.Repositories;

public sealed class OutboxRepository(
    FinanceDbContext dbContext) : IOutboxRepository
{
    public async Task AddAsync(
        OutboxMessage message, CancellationToken ct = default)
    {
        await dbContext.OutboxMessages.AddAsync(message, ct);
    }

    public async Task<IReadOnlyList<Guid>> ClaimBatchAsync(
        string workerId,
        int batchSize,
        TimeSpan leaseDuration,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workerId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);

        if (leaseDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(leaseDuration));

        var leaseSeconds = (int)Math.Ceiling(leaseDuration.TotalSeconds);

        var ids = await dbContext.Database
        .SqlQueryRaw<Guid>(
            """
            WITH candidates AS (
                SELECT id
                FROM outbox_messages
                WHERE published_at IS NULL
                  AND next_attempt_at <= NOW()
                  AND (
                      locked_until IS NULL
                      OR locked_until <= NOW()
                  )
                ORDER BY next_attempt_at, created_at, id
                LIMIT {0}
                FOR UPDATE SKIP LOCKED
            )
            UPDATE outbox_messages AS o
            SET locked_by = {1},
                locked_until = NOW() + ({2} * INTERVAL '1 second')
            FROM candidates AS c
            WHERE o.id = c.id
            RETURNING o.id AS "Value"
            """,
            batchSize,
            workerId,
            leaseSeconds)
        .ToListAsync(ct);

        return ids;
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetClaimedAsync(
        string workerId,
        IReadOnlyCollection<Guid> messageIds,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workerId);

        if (messageIds.Count == 0)
            return [];

        return await dbContext.OutboxMessages
            .AsNoTracking()
            .Where(x =>
                messageIds.Contains(x.Id) &&
                x.LockedBy == workerId &&
                x.PublishedAt == null &&
                x.LockedUntil > DateTime.UtcNow)
            .ToListAsync(ct);
    }


    public async Task<bool> MarkPublishedAsync(
        Guid messageId,
        string workerId,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workerId);

        var affectedRows = await dbContext.OutboxMessages
            .Where(x =>
                x.Id == messageId &&
                x.LockedBy == workerId &&
                x.LockedUntil > DateTime.UtcNow &&
                x.PublishedAt == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.PublishedAt, DateTime.UtcNow)
                .SetProperty(x => x.LockedBy, (string?)null)
                .SetProperty(x => x.LockedUntil, (DateTime?)null),
                ct);

        return affectedRows == 1;
    }

    public async Task<bool> MarkFailedAsync(
        Guid messageId,
        string workerId,
        string error,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workerId);
        ArgumentNullException.ThrowIfNull(error);

        var now = DateTime.UtcNow;

        var affectedRows = await dbContext.OutboxMessages
            .Where(x =>
                x.Id == messageId &&
                x.LockedBy == workerId &&
                x.LockedUntil > now &&
                x.PublishedAt == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Attempts, x => x.Attempts + 1)
                .SetProperty(
                    x => x.NextAttemptAt,
                    x => now.AddMinutes(1))
                .SetProperty(x => x.LastError,
                    error.Length > 2000 ? error[..2000] : error)
                .SetProperty(x => x.LockedBy, (string?)null)
                .SetProperty(x => x.LockedUntil, (DateTime?)null),
                ct);

        return affectedRows == 1;
    }

}