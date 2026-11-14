using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Domain.Ledger.Entities;

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
}