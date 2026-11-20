using AtlasPay.Finance.Domain.Accounts.Entities;

namespace AtlasPay.Finance.Application.Abstractions.Persistence;

public interface IAccountRepository
{
    Task AddAsync(
        Account account,
        CancellationToken cancellationToken = default);

    Task<Account?> GetByIdAsync(
        Guid accountId,
        Guid customerId,
        CancellationToken cancellationToken = default);
}
