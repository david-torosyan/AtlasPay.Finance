using AtlasPay.Finance.Application.Abstractions.Identity;
using AtlasPay.Finance.Application.Abstractions.Persistence;

namespace AtlasPay.Finance.Application.Accounts.Queries.GetAccount;

public class GetAccountHandler
{
    private readonly IAccountReadRepository _repository;
    private readonly ICurrentCustomer _currentCustomer;

    public GetAccountHandler(
        IAccountReadRepository repository,
        ICurrentCustomer currentCustomer)
    {
        _repository = repository;
        _currentCustomer = currentCustomer;
    }

    public async Task<GetAccountResult?> HandleAsync(
        GetAccountQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.AccountId == Guid.Empty)
            throw new ArgumentException(
                "Account ID cannot be empty.",
                nameof(query));

        return await _repository.GetByIdAsync(
            query.AccountId,
            _currentCustomer.CustomerId,
            cancellationToken);
    }
}
