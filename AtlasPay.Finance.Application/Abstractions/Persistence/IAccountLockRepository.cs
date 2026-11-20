using AtlasPay.Finance.Domain.Accounts.Entities;

namespace AtlasPay.Finance.Application.Abstractions.Persistence;

public interface IAccountLockRepository
{
    Task<IReadOnlyList<Account>> LockAccountsAsync(
        Guid firstAccountId,
        Guid secondAccountId,
        CancellationToken ct = default);

    Task<IReadOnlyList<Account>> LockAccountsAsync(
        Guid firstAccountId,
        Guid secondAccountId,
        Guid sourceCustomerId,
        CancellationToken ct = default);

    Task<Account> LockAccountAsync(
        Guid accountId,
        CancellationToken ct = default);

    Task<Account> LockAccountAsync(
        Guid accountId,
        Guid customerId,
        CancellationToken ct = default);
}
