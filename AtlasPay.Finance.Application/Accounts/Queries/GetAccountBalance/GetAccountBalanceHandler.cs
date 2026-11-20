using AtlasPay.Finance.Application.Abstractions.Identity;
using AtlasPay.Finance.Application.Abstractions.Persistence;

namespace AtlasPay.Finance.Application.Accounts.Queries.GetAccountBalance;

public sealed class GetAccountBalanceHandler(
    IAccountReadRepository repository,
    ICurrentCustomer currentCustomer)
{
    public Task<GetAccountBalanceResult?> HandleAsync(
        GetAccountBalanceQuery query,
        CancellationToken ct = default)
    {
        if (query.AccountId == Guid.Empty)
            throw new ArgumentException(
                "Account ID cannot be empty.",
                nameof(query));

        return repository.GetBalanceByIdAsync(
            query.AccountId,
            currentCustomer.CustomerId,
            ct);
    }
}
