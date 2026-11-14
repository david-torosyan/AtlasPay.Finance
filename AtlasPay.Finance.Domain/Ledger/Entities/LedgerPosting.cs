using AtlasPay.Finance.Domain.Ledger.ValueObjects;

namespace AtlasPay.Finance.Domain.Ledger.Entities;

public class LedgerPosting
{
    private LedgerPosting() { } // For EF Core

    private LedgerPosting(
        Guid ledgerTransactionId,
        Guid ledgerAccountId,
        PostingDirection direction,
        Money amount)
    {
        Id = Guid.NewGuid();
        LedgerTransactionId = ledgerTransactionId;
        LedgerAccountId = ledgerAccountId;
        Direction = direction;
        Amount = amount;
    }

    public Guid Id { get; private set; }

    public Guid LedgerTransactionId { get; private set; }

    public Guid LedgerAccountId { get; private set; }

    public PostingDirection Direction { get; private set; }

    public Money Amount { get; private set; } = null!;

    public static LedgerPosting Create(
        Guid ledgerTransactionId,
        Guid ledgerAccountId,
        PostingDirection direction,
        Money amount)
    {
        if (ledgerTransactionId == Guid.Empty)
            throw new ArgumentException(
                "Ledger transaction ID cannot be empty.",
                nameof(ledgerTransactionId));

        if (ledgerAccountId == Guid.Empty)
            throw new ArgumentException(
                "Ledger account ID cannot be empty.",
                nameof(ledgerAccountId));

        if (!Enum.IsDefined(direction))
            throw new ArgumentException(
                "Unsupported posting direction.",
                nameof(direction));

        ArgumentNullException.ThrowIfNull(amount);

        if (amount.Amount <= 0)
            throw new ArgumentException(
                "Posting amount must be greater than zero.",
                nameof(amount));

        return new LedgerPosting(
            ledgerTransactionId,
            ledgerAccountId,
            direction,
            amount);
    }
}