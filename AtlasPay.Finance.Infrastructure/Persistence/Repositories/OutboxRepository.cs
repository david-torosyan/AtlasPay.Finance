
using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Domain.Outbox.Entities;

namespace AtlasPay.Finance.Infrastructure.Persistence.Repositories;

public sealed class OutboxRepository(
    FinanceDbContext dbContext) : IOutboxRepository
{
    public async Task AddAsync(
        OutboxMessage message, CancellationToken ct = default)
    {
        await dbContext.OutboxMessages.AddAsync(message, ct);
    }
}