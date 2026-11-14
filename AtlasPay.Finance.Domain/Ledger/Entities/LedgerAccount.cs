using AtlasPay.Finance.Domain.Accounts.ValueObjects;
using AtlasPay.Finance.Domain.Ledger.ValueObjects;

namespace AtlasPay.Finance.Domain.Ledger.Entities;

public class LedgerAccount
{
    private LedgerAccount() { }
    private LedgerAccount(
        Guid? accountId, LedgerAccountType accountType, Currency currency)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        Type = accountType;
        Currency = currency;
    }

    public Guid Id { get; private set; }
    public Guid? AccountId { get; private set; }
    public LedgerAccountType Type { get; private set; }
    public Currency Currency { get; private set; }

    public static LedgerAccount Create(
        Guid? accountId,
        LedgerAccountType type,
        Currency currency)
    {
        if (accountId.HasValue && accountId.Value == Guid.Empty)
            throw new ArgumentException(
                "Account ID cannot be empty.",
                nameof(accountId));

        if (!Enum.IsDefined(type))
            throw new ArgumentException(
                "Unsupported account type.",
                nameof(type));

        if (!Enum.IsDefined(currency))
            throw new ArgumentException(
                "Unsupported currency.",
                nameof(currency));

        return new LedgerAccount(accountId, type, currency);
    }
}
