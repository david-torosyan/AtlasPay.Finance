using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Domain.Withdrawals.Entities;

using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Finance.Infrastructure.Persistence.Repositories;

public class WithdrawalRepository
    (FinanceDbContext _dbContext) : IWithdrawalRepository
{
    public async Task AddAsync(
        Withdrawal deposit, CancellationToken ct = default)
    {
        await _dbContext.Withdrawals
                    .AddAsync(deposit, ct);
    }

    public Task<Withdrawal?> GetByRequestIdAsync(
        Guid requestId,
        Guid customerId,
        CancellationToken ct = default)
    {
        return _dbContext.Withdrawals
            .AsNoTracking()
            .SingleOrDefaultAsync(
                withdrawal =>
                    withdrawal.RequestId == requestId &&
                    _dbContext.Accounts.Any(account =>
                        account.Id == withdrawal.AccountId &&
                        account.CustomerId == customerId),
                ct);
    }
}
