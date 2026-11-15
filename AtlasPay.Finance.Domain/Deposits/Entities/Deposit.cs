
using AtlasPay.Finance.Domain.Deposits.ValueObjects;
using AtlasPay.Finance.Domain.Ledger.ValueObjects;

namespace AtlasPay.Finance.Domain.Deposits.Entities;

public sealed class Deposit
{
    private Deposit() { } // EF Core

    private Deposit(
        Guid requestId,
        Guid accountId,
        Money amount)
    {
        Id = Guid.NewGuid();
        RequestId = requestId;
        AccountId = accountId;
        Amount = amount;
        Status = DepositStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid RequestId { get; private set; }

    public Guid AccountId { get; private set; }

    public Money Amount { get; private set; } = null!;

    public DepositStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public static Deposit Create(
        Guid requestId,
        Guid accountId,
        Money amount)
    {
        if (requestId == Guid.Empty)
            throw new ArgumentException(
                "RequestId cannot be empty.");

        if (accountId == Guid.Empty)
            throw new ArgumentException(
                "AccountId cannot be empty.");

        ArgumentNullException.ThrowIfNull(amount);

        if (amount.Amount <= 0)
            throw new ArgumentException(
                "Deposit amount must be positive.");

        return new Deposit(requestId, accountId, amount);
    }

    public void Complete()
    {
        if (Status != DepositStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot complete deposit in {Status} status.");

        Status = DepositStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void Fail()
    {
        if (Status != DepositStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot fail deposit in {Status} status.");

        Status = DepositStatus.Failed;
    }
}
