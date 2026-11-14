using AtlasPay.Finance.Domain.Accounts.ValueObjects;

namespace AtlasPay.Finance.Domain.Accounts.Entities;

public class Account
{
    private Account() { } // For EF Core

    private Account(Guid customerId, Currency currency)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        Currency = currency;
        Status = AccountStatus.Pending;
    }

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public AccountStatus Status { get; private set; }
    public Currency Currency { get; private set; }

    public static Account Create(
        Guid customerId,
        Currency currency)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException(
                "Customer ID cannot be empty.",
                nameof(customerId));

        if (!Enum.IsDefined(currency))
            throw new ArgumentException(
                "Unsupported currency.",
                nameof(currency));

        return new Account(customerId, currency);
    }

    public void Activate()
    {
        if (Status != AccountStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot activate account in {Status} status.");

        Status = AccountStatus.Active;
    }

    public void Freeze()
    {
        if (Status != AccountStatus.Active)
            throw new InvalidOperationException(
                $"Cannot freeze account in {Status} status.");

        Status = AccountStatus.Frozen;
    }

    public void Unfreeze()
    {
        if (Status != AccountStatus.Frozen)
            throw new InvalidOperationException(
                $"Cannot unfreeze account in {Status} status.");

        Status = AccountStatus.Active;
    }

    public void Close()
    {
        if (Status != AccountStatus.Active)
            throw new InvalidOperationException(
                $"Cannot close account in {Status} status.");

        Status = AccountStatus.Closed;
    }
}
