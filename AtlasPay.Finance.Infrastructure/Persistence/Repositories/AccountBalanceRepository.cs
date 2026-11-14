using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Domain.Ledger.Entities;

namespace AtlasPay.Finance.Infrastructure.Persistence.Repositories;

public sealed class AccountBalanceRepository : IAccountBalanceRepository
{
    private readonly FinanceDbContext _dbContext;

    public AccountBalanceRepository(FinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        AccountBalance balance,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.AccountBalances.AddAsync(
            balance, cancellationToken);
    }
}