using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Application.Ledger.Queries.CheckLedgerIntegrity;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Finance.Infrastructure.Persistence.Repositories;

public sealed class LedgerIntegrityRepository(
    FinanceDbContext dbContext) : ILedgerIntegrityRepository
{
    public async Task<IReadOnlyList<UnbalancedTransactionItem>>
        GetUnbalancedTransactionsAsync(CancellationToken ct = default)
    {
        var rows = await dbContext.Database
            .SqlQueryRaw<UnbalancedTransactionRow>(
                "SELECT * FROM finance.check_unbalanced_transactions()")
            .ToListAsync(ct);

        return rows.Select(x => new UnbalancedTransactionItem(
            x.LedgerTransactionId,
            x.Currency,
            x.TotalDebit,
            x.TotalCredit,
            x.Difference)).ToList();
    }

    private sealed class UnbalancedTransactionRow
    {
        public Guid LedgerTransactionId { get; set; }
        public string Currency { get; set; } = null!;
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal Difference { get; set; }
    }
}