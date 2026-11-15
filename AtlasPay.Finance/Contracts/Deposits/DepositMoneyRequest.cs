using AtlasPay.Finance.Domain.Accounts.ValueObjects;

namespace AtlasPay.Finance.Api.Contracts.Deposits;

public sealed record DepositMoneyRequest(
    Guid RequestId,
    Guid AccountId,
    decimal Amount,
    Currency Currency);