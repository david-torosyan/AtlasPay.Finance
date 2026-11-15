using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Domain.Ledger.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Finance.Infrastructure.Persistence.Repositories;

public sealed class LedgerTransactionRepository(
    FinanceDbContext dbContext)
    : ILedgerTransactionRepository
{
    public async Task AddAsync(
        LedgerTransaction transaction,
        CancellationToken ct = default)
    {
        await dbContext.LedgerTransactions.AddAsync(
            transaction, ct);
    }

    public Task<LedgerTransaction?> GetByReferenceIdAsync(
        Guid referenceId,
        CancellationToken ct = default)
    {
        return dbContext.LedgerTransactions
            .AsNoTracking()
            .Include(x => x.Postings)
            .SingleOrDefaultAsync(
                x => x.ReferenceId == referenceId,
                ct);
    }
}