
namespace AtlasPay.Finance.Domain.Transfers.Entities;

public sealed class TransferReversal
{
    private TransferReversal() { } // EF Core

    private TransferReversal(
        Guid requestId,
        Guid originalTransferId,
        string reason)
    {
        Id = Guid.NewGuid();
        RequestId = requestId;
        OriginalTransferId = originalTransferId;
        Reason = reason;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid RequestId { get; private set; }

    public Guid OriginalTransferId { get; private set; }

    public string Reason { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public static TransferReversal Create(
        Guid requestId,
        Guid originalTransferId,
        string reason)
    {
        if (requestId == Guid.Empty)
            throw new ArgumentException(
                "RequestId is required.",
                nameof(requestId));

        if (originalTransferId == Guid.Empty)
            throw new ArgumentException(
                "OriginalTransferId is required.",
                nameof(originalTransferId));

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException(
                "Reversal reason is required.",
                nameof(reason));

        reason = reason.Trim();

        if (reason.Length > 500)
            throw new ArgumentException(
                "Reason cannot exceed 500 characters.",
                nameof(reason));

        return new TransferReversal(
            requestId,
            originalTransferId,
            reason);
    }
}
