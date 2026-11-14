using AtlasPay.Finance.Domain.Accounts.ValueObjects;

namespace AtlasPay.Finance.Domain.Ledger.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    public Money(decimal amount, Currency currency)
    {
        if (!Enum.IsDefined(currency))
            throw new ArgumentException(
                "Unsupported currency.", nameof(currency));

        if (decimal.Round(amount, 2) != amount)
            throw new ArgumentException(
                "Amount cannot have more than 2 decimal places.",
                nameof(amount));

        Amount = amount;
        Currency = currency;
    }

    public static Money operator +(Money a, Money b)
    {
        EnsureSameCurrency(a, b);

        return new Money(
            checked(a.Amount + b.Amount),
            a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        EnsureSameCurrency(a, b);

        return new Money(
            checked(a.Amount - b.Amount),
            a.Currency);
    }

    private static void EnsureSameCurrency(Money a, Money b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        if (a.Currency != b.Currency)
            throw new InvalidOperationException(
                "Cannot perform arithmetic on different currencies.");
    }
}