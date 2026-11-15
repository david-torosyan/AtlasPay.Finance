using AtlasPay.Finance.Domain.Outbox.Entities;

namespace AtlasPay.Finance.Application.Abstractions.Persistence;

public interface IOutboxRepository
{
    Task AddAsync(
        OutboxMessage message,
        CancellationToken ct = default);

    Task<IReadOnlyList<Guid>> ClaimBatchAsync(
        string workerId,
        int batchSize,
        TimeSpan leaseDuration,
        CancellationToken ct = default);

    Task<IReadOnlyList<OutboxMessage>> GetClaimedAsync(
        string workerId,
        IReadOnlyCollection<Guid> messageIds,
        CancellationToken ct = default);

    Task<bool> MarkPublishedAsync(
        Guid messageId,
        string workerId,
        CancellationToken ct = default);

    Task<bool> MarkFailedAsync(
        Guid messageId,
        string workerId,
        string error,
        CancellationToken ct = default);
}