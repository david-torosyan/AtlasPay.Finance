using AtlasPay.Finance.Application.Transfers.Queries.GetTransfer;

namespace AtlasPay.Finance.Application.Transfers.Queries.GetAccountTransactions;

public sealed record GetAccountTransactionsResult(
    IReadOnlyList<GetTransferResult> Items,
    int Page,
    int PageSize,
    int TotalCount);