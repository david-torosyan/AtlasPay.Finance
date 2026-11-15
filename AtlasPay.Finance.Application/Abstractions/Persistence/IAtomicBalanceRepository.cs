using AtlasPay.Finance.Domain.Ledger.ValueObjects;

namespace AtlasPay.Finance.Application.Abstractions.Persistence;

public interface IAtomicBalanceRepository
{
    Task IncreaseAsync(
        Guid ledgerAccountId,
        Money amount,
        CancellationToken ct = default);

    Task<bool> TryDecreaseAsync(
        Guid ledgerAccountId,
        Money amount,
        CancellationToken ct = default);
}