using AtlasPay.Finance.Domain.Ledger.ValueObjects;

namespace AtlasPay.Finance.Domain.Ledger.Entities;

public class LedgerTransaction
{
    private readonly List<LedgerPosting> _postings = [];

    private LedgerTransaction() { } // For EF Core

    private LedgerTransaction(
        Guid referenceId,
        LedgerTransactionType type)
    {
        Id = Guid.NewGuid();
        ReferenceId = referenceId;
        Type = type;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid ReferenceId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public bool IsPosted { get; private set; }

    public IReadOnlyCollection<LedgerPosting> Postings =>
        _postings.AsReadOnly();

    public LedgerTransactionType Type { get; private set; }
    public Guid? ReversesTransactionId { get; private set; }

    public static LedgerTransaction Create(
        Guid referenceId,
        LedgerTransactionType type)
    {
        if (referenceId == Guid.Empty)
            throw new ArgumentException(
                "ReferenceId cannot be empty.",
                nameof(referenceId));

        if (!Enum.IsDefined(type))
            throw new ArgumentOutOfRangeException(nameof(type));

        if (type == LedgerTransactionType.Reversal)
            throw new InvalidOperationException(
                "Use CreateReversal for reversal transactions.");

        return new LedgerTransaction(referenceId, type);
    }

    public static LedgerTransaction CreateReversal(
        Guid reversalId,
        LedgerTransaction original)
    {
        if (reversalId == Guid.Empty)
            throw new ArgumentException(
                "Reversal ID cannot be empty.",
                nameof(reversalId));

        ArgumentNullException.ThrowIfNull(original);

        if (!original.IsPosted)
            throw new InvalidOperationException(
                "Only posted transactions can be reversed.");

        if (original.Type != LedgerTransactionType.Transfer)
            throw new InvalidOperationException(
                "Only transfer reversals are currently supported.");

        var reversal = new LedgerTransaction(
            reversalId,
            LedgerTransactionType.Reversal)
        {
            ReversesTransactionId = original.Id
        };

        foreach (var posting in original.Postings)
        {
            var oppositeDirection = posting.Direction switch
            {
                PostingDirection.Debit =>
                    PostingDirection.Credit,

                PostingDirection.Credit =>
                    PostingDirection.Debit,

                _ => throw new InvalidOperationException(
                    "Invalid posting direction.")
            };

            reversal.AddPosting(
                posting.LedgerAccountId,
                oppositeDirection,
                posting.Amount);
        }

        reversal.Post();

        return reversal;
    }

    public void AddPosting(
       Guid ledgerAccountId,
       PostingDirection direction,
       Money amount)
    {
        if (IsPosted)
            throw new InvalidOperationException(
                "Cannot add postings to a finalized transaction.");

        var posting = LedgerPosting.Create(
            Id,
            ledgerAccountId,
            direction,
            amount);

        _postings.Add(posting);
    }

    public void Post()
    {
        if (IsPosted)
            throw new InvalidOperationException(
                "Transaction has already been posted.");

        if (_postings.Count < 2)
            throw new InvalidOperationException(
                "A ledger transaction requires at least two postings.");

        foreach (var currencyGroup in _postings.GroupBy(
                     p => p.Amount.Currency))
        {
            decimal totalDebits = currencyGroup
                .Where(p => p.Direction == PostingDirection.Debit)
                .Sum(p => p.Amount.Amount);

            decimal totalCredits = currencyGroup
                .Where(p => p.Direction == PostingDirection.Credit)
                .Sum(p => p.Amount.Amount);

            if (totalDebits != totalCredits)
                throw new InvalidOperationException(
                    $"Unbalanced ledger transaction in {currencyGroup.Key}. " +
                    $"Debits: {totalDebits}, Credits: {totalCredits}.");
        }

        IsPosted = true;
    }
}
