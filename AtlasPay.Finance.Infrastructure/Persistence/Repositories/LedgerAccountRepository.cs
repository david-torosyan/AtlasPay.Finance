using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Domain.Ledger.Entities;

namespace AtlasPay.Finance.Infrastructure.Persistence.Repositories;

public sealed class LedgerAccountRepository : ILedgerAccountRepository
{
    private readonly FinanceDbContext _dbContext;

    public LedgerAccountRepository(FinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        LedgerAccount ledgerAccount,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.LedgerAccounts.AddAsync(
            ledgerAccount, cancellationToken);
    }
}