using AtlasPay.Finance.Domain.Accounts.ValueObjects;
using AtlasPay.Finance.Domain.Ledger.ValueObjects;

namespace AtlasPay.Finance.Domain.Ledger.Entities;

public class AccountBalance
{
    private AccountBalance() { } // For EF Core
    private AccountBalance(
        Guid ledgerAccountId, Currency currency)
    {
        LedgerAccountId = ledgerAccountId;
        Balance = new Money(0m, currency);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid LedgerAccountId { get; private set; }

    public Money Balance { get; private set; } = null!;

    public DateTimeOffset UpdatedAt { get; private set; }

    public static AccountBalance Create(
    Guid ledgerAccountId,
    Currency currency)
    {
        if (ledgerAccountId == Guid.Empty)
            throw new ArgumentException("Ledger account ID cannot be empty.",
            nameof(ledgerAccountId));

        return new AccountBalance(ledgerAccountId, currency);
    }

    public void Debit(
        Money amount,
        LedgerAccountType accountType)
    {
        Apply(amount, accountType, PostingDirection.Debit);
    }

    public void Credit(
        Money amount,
        LedgerAccountType accountType)
    {
        Apply(amount, accountType, PostingDirection.Credit);
    }

    private void Apply(
        Money amount,
        LedgerAccountType accountType,
        PostingDirection direction)
    {
        ValidateAmount(amount);

        if (!Enum.IsDefined(accountType))
            throw new ArgumentOutOfRangeException(nameof(accountType));

        if (!Enum.IsDefined(direction))
            throw new ArgumentOutOfRangeException(nameof(direction));

        var debitIncreases =
            accountType is LedgerAccountType.Asset
                or LedgerAccountType.Expense;

        var increase = direction == PostingDirection.Debit
            ? debitIncreases
            : !debitIncreases;
        
        var newAmount = increase
            ? Balance.Amount + amount.Amount
            : Balance.Amount - amount.Amount;

        Balance = new Money(newAmount, Balance.Currency);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private void ValidateAmount(Money amount)
    {
        ArgumentNullException.ThrowIfNull(amount);

        if (amount.Amount <= 0)
            throw new ArgumentException(
                "Amount must be greater than zero.",
                nameof(amount));

        if (Balance.Currency != amount.Currency)
            throw new InvalidOperationException(
                "Currency mismatch.");
    }
}
