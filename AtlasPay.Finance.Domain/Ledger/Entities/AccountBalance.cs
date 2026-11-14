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
}
