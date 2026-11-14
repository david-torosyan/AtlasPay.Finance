using AtlasPay.Finance.Domain.Accounts.Entities;

namespace AtlasPay.Finance.Application.Abstractions.Persistence;

public interface IAccountLockRepository
{
    Task<IReadOnlyList<Account>> LockAccountsAsync(
        Guid firstAccountId,
        Guid secondAccountId,
        CancellationToken ct = default);
}