using AtlasPay.Finance.Application.Abstractions.Persistence;

namespace AtlasPay.Finance.Application.Ledger.Queries.CheckLedgerIntegrity;

public sealed class CheckLedgerIntegrityHandler(
    ILedgerIntegrityRepository repository)
{
    public async Task<CheckLedgerIntegrityResult> HandleAsync(
        CancellationToken ct = default)
    {
        var items = await repository.GetUnbalancedTransactionsAsync(ct);

        return new CheckLedgerIntegrityResult(items.Count, items);
    }
}