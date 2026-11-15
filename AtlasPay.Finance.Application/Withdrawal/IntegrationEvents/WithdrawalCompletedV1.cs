namespace AtlasPay.Finance.Application.Withdrawals.IntegrationEvents;

public sealed record WithdrawalCompletedV1(
    Guid EventId,
    Guid WithdrawalId,
    Guid AccountId,
    decimal Amount,
    string Currency,
    DateTimeOffset OccurredAt);