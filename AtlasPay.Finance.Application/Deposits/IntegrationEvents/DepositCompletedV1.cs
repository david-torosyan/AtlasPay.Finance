namespace AtlasPay.Finance.Application.Deposits.IntegrationEvents;

public sealed record DepositCompletedV1(
    Guid EventId,
    Guid DepositId,
    Guid AccountId,
    decimal Amount,
    string Currency);
