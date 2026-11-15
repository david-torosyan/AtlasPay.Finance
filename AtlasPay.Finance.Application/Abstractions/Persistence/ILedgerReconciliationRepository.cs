using AtlasPay.Finance.Application.Ledger.Queries.ReconcileLedger;

namespace AtlasPay.Finance.Application.Abstractions.Persistence;

public interface ILedgerReconciliationRepository
{
    Task<IReadOnlyList<LedgerReconciliationItem>>
        ReconcileAsync(CancellationToken ct = default);
}