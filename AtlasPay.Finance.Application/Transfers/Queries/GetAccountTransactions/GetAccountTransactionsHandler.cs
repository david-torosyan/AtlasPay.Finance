using AtlasPay.Finance.Application.Abstractions.Persistence;

namespace AtlasPay.Finance.Application.Transfers.Queries.GetAccountTransactions;

public sealed class GetAccountTransactionsHandler(
    ITransferReadRepository repository)
{
    public Task<GetAccountTransactionsResult> HandleAsync(
        GetAccountTransactionsQuery query,
        CancellationToken ct = default)
    {
        if (query.AccountId == Guid.Empty)
            throw new ArgumentException(
                "Account ID cannot be empty.");

        if (query.Page < 1 || query.Page > 1_000_000)
            throw new ArgumentException(
                "Page must be between 1 and 1,000,000.");

        if (query.PageSize < 1 || query.PageSize > 100)
            throw new ArgumentException(
                "PageSize must be between 1 and 100.");

        return repository.GetByAccountIdAsync(
            query.AccountId,
            query.Page,
            query.PageSize,
            ct);
    }
}