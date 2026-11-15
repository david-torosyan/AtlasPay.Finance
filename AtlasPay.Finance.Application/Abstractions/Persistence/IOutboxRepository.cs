using AtlasPay.Finance.Domain.Outbox.Entities;

namespace AtlasPay.Finance.Application.Abstractions.Persistence;

public interface IOutboxRepository
{
    Task AddAsync(
        OutboxMessage message,
        CancellationToken ct = default);
}