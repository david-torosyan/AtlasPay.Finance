using AtlasPay.Finance.Domain.Accounts.ValueObjects;
using AtlasPay.Finance.Domain.Ledger.ValueObjects;

namespace AtlasPay.Finance.Domain.Ledger.Entities;

public class LedgerAccount
{
    private LedgerAccount() { }
    private LedgerAccount(
        Guid? accountId, LedgerAccountType accountType, Currency currency, string? code)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        Type = accountType;
        Currency = currency;
        Code = code;
    }

    public Guid Id { get; private set; }
    public Guid? AccountId { get; private set; }
    public LedgerAccountType Type { get; private set; }
    public Currency Currency { get; private set; }
    public string? Code { get; private set; }

    public static LedgerAccount Create(
        Guid? accountId,
        LedgerAccountType type,
        Currency currency)
    {
        if (accountId is null || accountId == Guid.Empty)
            throw new ArgumentException(
                "Customer ledger account must reference an Account.");

        ValidateTypeAndCurrency(type, currency);

        return new LedgerAccount(
            accountId,
            type,
            currency,
            null);
    }

    public static LedgerAccount CreateInternal(
        string code,
        LedgerAccountType type,
        Currency currency)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException(
                "Internal ledger account code is required.",
                nameof(code));

        if (code.Length > 64)
            throw new ArgumentException(
                "Ledger account code cannot exceed 64 characters.",
                nameof(code));

        ValidateTypeAndCurrency(type, currency);

        return new LedgerAccount(
            null,
            type,
            currency,
            code);
    }

    private static void ValidateTypeAndCurrency(
        LedgerAccountType type,
        Currency currency)
    {
        if (!Enum.IsDefined(type))
            throw new ArgumentOutOfRangeException(nameof(type));

        if (!Enum.IsDefined(currency))
            throw new ArgumentOutOfRangeException(nameof(currency));
    }
}
