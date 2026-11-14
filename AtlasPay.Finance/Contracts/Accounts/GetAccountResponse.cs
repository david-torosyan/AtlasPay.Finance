namespace AtlasPay.Finance.Contracts.Accounts;

public sealed record GetAccountResponse(
    Guid AccountId,
    Guid CustomerId,
    string Currency,
    string Status,
    decimal Balance);