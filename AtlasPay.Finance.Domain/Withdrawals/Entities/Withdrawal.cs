
using AtlasPay.Finance.Domain.Ledger.ValueObjects;
using AtlasPay.Finance.Domain.Withdrawals.ValueObjects;

namespace AtlasPay.Finance.Domain.Withdrawals.Entities;

public sealed class Withdrawal
{
    private Withdrawal() { } // EF Core

    private Withdrawal(
        Guid requestId,
        Guid accountId,
        Money amount)
    {
        Id = Guid.NewGuid();
        RequestId = requestId;
        AccountId = accountId;
        Amount = amount;
        Status = WithdrawalStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid RequestId { get; private set; }

    public Guid AccountId { get; private set; }

    public Money Amount { get; private set; } = null!;

    public WithdrawalStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public static Withdrawal Create(
        Guid requestId,
        Guid accountId,
        Money amount)
    {
        if (requestId == Guid.Empty)
            throw new ArgumentException(
                "RequestId cannot be empty.",
                nameof(requestId));

        if (accountId == Guid.Empty)
            throw new ArgumentException(
                "AccountId cannot be empty.",
                nameof(accountId));

        ArgumentNullException.ThrowIfNull(amount);

        if (amount.Amount <= 0)
            throw new ArgumentException(
                "Withdrawal amount must be positive.",
                nameof(amount));

        return new Withdrawal(requestId, accountId, amount);
    }

    public void Complete()
    {
        if (Status != WithdrawalStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot complete withdrawal in {Status} status.");

        Status = WithdrawalStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void Fail()
    {
        if (Status != WithdrawalStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot fail withdrawal in {Status} status.");

        Status = WithdrawalStatus.Failed;
    }
}
