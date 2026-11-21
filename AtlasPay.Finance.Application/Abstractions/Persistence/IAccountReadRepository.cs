using AtlasPay.Finance.Application.Accounts.Queries.GetAccount;
using AtlasPay.Finance.Application.Accounts.Queries.GetAccountBalance;

namespace AtlasPay.Finance.Application.Abstractions.Persistence;

public interface IAccountReadRepository
{
    Task<GetAccountResult?> GetByIdAsync(
        Guid accountId,
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GetAccountResult>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task<GetAccountBalanceResult?> GetBalanceByIdAsync(
        Guid accountId,
        Guid customerId,
        CancellationToken cancellationToken = default);
}
