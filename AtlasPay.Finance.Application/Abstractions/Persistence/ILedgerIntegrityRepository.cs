using AtlasPay.Finance.Application.Ledger.Queries.CheckLedgerIntegrity;

namespace AtlasPay.Finance.Application.Abstractions.Persistence;

public interface ILedgerIntegrityRepository
{
    Task<IReadOnlyList<UnbalancedTransactionItem>> GetUnbalancedTransactionsAsync(
        CancellationToken ct = default);
}