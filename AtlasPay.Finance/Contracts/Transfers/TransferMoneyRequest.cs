using AtlasPay.Finance.Domain.Accounts.ValueObjects;

namespace AtlasPay.Finance.Contracts.Transfers;

public sealed record TransferMoneyRequest(
    Guid RequestId,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    Currency Currency);