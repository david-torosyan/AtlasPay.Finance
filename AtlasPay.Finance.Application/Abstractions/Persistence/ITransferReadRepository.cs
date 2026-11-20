using AtlasPay.Finance.Application.Transfers.Queries.GetTransfer;
using AtlasPay.Finance.Application.Transfers.Queries.GetAccountTransactions;

namespace AtlasPay.Finance.Application.Abstractions.Persistence;

public interface ITransferReadRepository
{
    Task<GetTransferResult?> GetByIdAsync(
        Guid transferId,
        Guid customerId,
        CancellationToken ct = default);

    Task<GetAccountTransactionsResult> GetByAccountIdAsync(
        Guid accountId,
        Guid customerId,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
