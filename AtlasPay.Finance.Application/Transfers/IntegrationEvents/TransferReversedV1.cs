namespace AtlasPay.Finance.Application.Transfers.IntegrationEvents;

public sealed record TransferReversedV1(
    Guid EventId,
    Guid ReversalId,
    Guid OriginalTransferId,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    string Currency,
    DateTimeOffset OccurredAt);