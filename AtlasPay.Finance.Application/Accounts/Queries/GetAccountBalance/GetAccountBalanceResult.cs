using AtlasPay.Finance.Domain.Accounts.ValueObjects;

namespace AtlasPay.Finance.Application.Accounts.Queries.GetAccountBalance;

public sealed record GetAccountBalanceResult(
    Guid AccountId,
    Currency Currency,
    decimal LedgerBalance);
