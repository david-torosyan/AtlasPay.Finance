using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Domain.Withdrawals.Entities;

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

    public async Task<Withdrawal?> GetByRequestIdAsync(
        Guid requestId, CancellationToken ct = default)
    {
        return _dbContext.Withdrawals
                        .SingleOrDefault(a => a.RequestId == requestId);
    }
}
