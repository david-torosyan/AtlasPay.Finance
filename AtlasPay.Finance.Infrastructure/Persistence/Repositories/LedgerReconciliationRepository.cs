using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Application.Ledger.Queries.ReconcileLedger;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Finance.Infrastructure.Persistence.Repositories;

public sealed class LedgerReconciliationRepository(
    FinanceDbContext dbContext) : ILedgerReconciliationRepository
{
    public async Task<IReadOnlyList<LedgerReconciliationItem>> ReconcileAsync(
        CancellationToken ct = default)
    {
        var rows = await dbContext.Database
            .SqlQueryRaw<ReconciliationRow>(
                "SELECT * FROM finance.reconcile_ledger()")
            .ToListAsync(ct);

        return rows.Select(x => new LedgerReconciliationItem(
            x.LedgerAccountId,
            x.Code,
            x.AccountType,
            x.Currency,
            x.ExpectedBalance,
            x.StoredBalance,
            x.Difference,
            x.IsMatch)).ToList();
    }

    private sealed class ReconciliationRow
    {
        public Guid LedgerAccountId { get; set; }
        public string? Code { get; set; }
        public string AccountType { get; set; } = null!;
        public string Currency { get; set; } = null!;
        public decimal ExpectedBalance { get; set; }
        public decimal? StoredBalance { get; set; }
        public decimal? Difference { get; set; }
        public bool IsMatch { get; set; }
    }
}