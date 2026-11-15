
namespace AtlasPay.Finance.Application.Transfers.IntegrationEvents;

public sealed record TransferCompletedV1(
    Guid EventId,
    Guid TransferId,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    string Currency,
    DateTimeOffset OccurredAt);
