using AtlasPay.Finance.Domain.Accounts.ValueObjects;

namespace AtlasPay.Finance.Application.Accounts.Queries.GetAccount;

public sealed record GetAccountResult(
    Guid AccountId,
    Guid CustomerId,
    Currency Currency,
    AccountStatus Status,
    decimal Balance);