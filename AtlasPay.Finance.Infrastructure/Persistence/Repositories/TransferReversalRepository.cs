
using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Domain.Transfers.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Finance.Infrastructure.Persistence.Repositories;

public sealed class TransferReversalRepository(
    FinanceDbContext dbContext)
    : ITransferReversalRepository
{
    public async Task AddAsync(
        TransferReversal reversal,
        CancellationToken ct = default)
    {
        await dbContext.TransferReversals.AddAsync(
            reversal, ct);
    }

    public Task<TransferReversal?> GetByRequestIdAsync(
        Guid requestId,
        CancellationToken ct = default)
    {
        return dbContext.TransferReversals
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.RequestId == requestId,
                ct);
    }

    public Task<TransferReversal?> GetByOriginalTransferIdAsync(
        Guid transferId,
        CancellationToken ct = default)
    {
        return dbContext.TransferReversals
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.OriginalTransferId == transferId,
                ct);
    }
}
