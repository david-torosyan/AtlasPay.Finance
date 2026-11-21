using AtlasPay.Finance.Application.Abstractions.Identity;
using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Application.Accounts.Queries.GetAccount;

namespace AtlasPay.Finance.Application.Accounts.Queries.GetAccounts;

public sealed class GetAccountsHandler
{
    private readonly IAccountReadRepository _repository;
    private readonly ICurrentCustomer _currentCustomer;

    public GetAccountsHandler(
        IAccountReadRepository repository,
        ICurrentCustomer currentCustomer)
    {
        _repository = repository;
        _currentCustomer = currentCustomer;
    }

    public Task<IReadOnlyList<GetAccountResult>> HandleAsync(
        GetAccountsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        return _repository.GetByCustomerIdAsync(
            _currentCustomer.CustomerId,
            cancellationToken);
    }
}
