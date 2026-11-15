using AtlasPay.Finance.Domain.Accounts.ValueObjects;

namespace AtlasPay.Finance.Application.Withdrawals.Commands;

public sealed record WithdrawalMoneyCommand(
    Guid RequestId,
    Guid AccountId,
    decimal Amount,
    Currency Currency);
