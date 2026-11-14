using AtlasPay.Finance.Domain.Ledger.Entities;

namespace AtlasPay.Finance.Application.Abstractions.Persistence;

public interface IAccountBalanceRepository
{
    Task AddAsync(
        AccountBalance balance,
        CancellationToken cancellationToken = default);
}