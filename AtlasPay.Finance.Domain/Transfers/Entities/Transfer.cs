using AtlasPay.Finance.Domain.Ledger.ValueObjects;
using AtlasPay.Finance.Domain.Transfers.ValueObjects;

namespace AtlasPay.Finance.Domain.Transfers.Entities;

public sealed class Transfer
{
    private Transfer() { } // For EF Core

    private Transfer(
        Guid fromAccountId,
        Guid toAccountId,
        Money amount)
    {
        Id = Guid.NewGuid();
        FromAccountId = fromAccountId;
        ToAccountId = toAccountId;
        Amount = amount;
        Status = TransferStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid FromAccountId { get; private set; }

    public Guid ToAccountId { get; private set; }

    public Money Amount { get; private set; } = null!;

    public TransferStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public static Transfer Create(
        Guid fromAccountId,
        Guid toAccountId,
        Money amount)
    {
        if (fromAccountId == Guid.Empty)
            throw new ArgumentException(
                "Source account ID cannot be empty.",
                nameof(fromAccountId));

        if (toAccountId == Guid.Empty)
            throw new ArgumentException(
                "Destination account ID cannot be empty.",
                nameof(toAccountId));

        if (fromAccountId == toAccountId)
            throw new ArgumentException(
                "Source and destination accounts must be different.");

        ArgumentNullException.ThrowIfNull(amount);

        if (amount.Amount <= 0)
            throw new ArgumentException(
                "Transfer amount must be greater than zero.",
                nameof(amount));

        return new Transfer(
            fromAccountId,
            toAccountId,
            amount);
    }

    public void Complete()
    {
        if (Status != TransferStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot complete transfer in {Status} status.");

        Status = TransferStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void Fail()
    {
        if (Status != TransferStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot fail transfer in {Status} status.");

        Status = TransferStatus.Failed;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        if (Status != TransferStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot cancel transfer in {Status} status.");

        Status = TransferStatus.Cancelled;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}