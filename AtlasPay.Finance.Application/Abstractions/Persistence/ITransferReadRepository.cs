using AtlasPay.Finance.Application.Transfers.Queries.GetTransfer;
using AtlasPay.Finance.Application.Transfers.Queries.GetAccountTransactions;

namespace AtlasPay.Finance.Application.Abstractions.Persistence;

public interface ITransferReadRepository
{
    Task<GetTransferResult?> GetByIdAsync(
        Guid transferId,
        CancellationToken ct = default);

    Task<GetAccountTransactionsResult> GetByAccountIdAsync(
        Guid accountId,
        int page,
        int pageSize,
        CancellationToken ct = default);
}