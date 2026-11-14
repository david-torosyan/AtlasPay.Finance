using AtlasPay.Finance.Domain.Accounts.ValueObjects;

namespace AtlasPay.Finance.Contracts.Accounts;

public sealed record CreateAccountRequest(
    Guid CustomerId,
    Currency Currency);