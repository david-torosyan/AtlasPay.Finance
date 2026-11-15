namespace AtlasPay.Finance.Application.Ledger.Queries.CheckLedgerIntegrity;

public sealed record CheckLedgerIntegrityResult(
    int UnbalancedTransactionCount,
    IReadOnlyList<UnbalancedTransactionItem> Items);