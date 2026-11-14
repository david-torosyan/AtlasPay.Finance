using AtlasPay.Finance.Application.Abstractions.Persistence;

namespace AtlasPay.Finance.Application.Accounts.Queries.GetAccount;

public class GetAccountHandler
{
    private readonly IAccountReadRepository _repository;

    public GetAccountHandler(IAccountReadRepository repository)
    {
        _repository = repository;
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
            cancellationToken);
    }
}
