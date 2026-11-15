namespace AtlasPay.Finance.Application.Transfers.Queries.GetAccountTransactions;

public sealed record GetAccountTransactionsQuery(
    Guid AccountId,
    int Page = 1,
    int PageSize = 20);