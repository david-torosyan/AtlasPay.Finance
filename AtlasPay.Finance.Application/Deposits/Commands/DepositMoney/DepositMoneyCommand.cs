using AtlasPay.Finance.Domain.Accounts.ValueObjects;

namespace AtlasPay.Finance.Application.Deposits.Commands.DepositMoney;

public sealed record DepositMoneyCommand(
    Guid RequestId,
    Guid AccountId,
    decimal Amount,
    Currency Currency);