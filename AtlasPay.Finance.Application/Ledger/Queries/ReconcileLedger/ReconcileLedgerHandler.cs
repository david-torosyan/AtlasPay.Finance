using AtlasPay.Finance.Application.Abstractions.Persistence;

namespace AtlasPay.Finance.Application.Ledger.Queries.ReconcileLedger;

public sealed class ReconcileLedgerHandler(
    ILedgerReconciliationRepository repository)
{
    public async Task<ReconcileLedgerResult> HandleAsync(
        CancellationToken ct = default)
    {
        var items = await repository.ReconcileAsync(ct);

        return new ReconcileLedgerResult(
            items.Count,
            items.Count(x => !x.IsMatch),
            items);
    }
}