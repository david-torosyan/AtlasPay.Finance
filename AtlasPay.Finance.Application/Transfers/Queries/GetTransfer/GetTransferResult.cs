using AtlasPay.Finance.Domain.Accounts.ValueObjects;
using AtlasPay.Finance.Domain.Transfers.ValueObjects;

namespace AtlasPay.Finance.Application.Transfers.Queries.GetTransfer;

public sealed record GetTransferResult(
    Guid Id,
    Guid RequestId,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    Currency Currency,
    TransferStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);