using AtlasPay.Finance.Domain.Accounts.ValueObjects;

namespace AtlasPay.Finance.Api.Contracts.Withdrawals;

public sealed record WithdrawalMoneyRequest(
    Guid RequestId,
    Guid AccountId,
    decimal Amount,
    Currency Currency);