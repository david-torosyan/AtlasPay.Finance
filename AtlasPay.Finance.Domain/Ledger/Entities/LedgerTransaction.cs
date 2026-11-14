using AtlasPay.Finance.Domain.Ledger.ValueObjects;

namespace AtlasPay.Finance.Domain.Ledger.Entities;

public class LedgerTransaction
{
    private readonly List<LedgerPosting> _postings = [];

    private LedgerTransaction() { } // For EF Core

    private LedgerTransaction(Guid referenceId)
    {
        Id = Guid.NewGuid();
        ReferenceId = referenceId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid ReferenceId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public bool IsPosted { get; private set; }

    public IReadOnlyCollection<LedgerPosting> Postings =>
        _postings.AsReadOnly();

    public static LedgerTransaction Create(Guid referenceId)
    {
        if (referenceId == Guid.Empty)
            throw new ArgumentException(
                "Reference ID cannot be empty.",
                nameof(referenceId));

        return new LedgerTransaction(referenceId);
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
