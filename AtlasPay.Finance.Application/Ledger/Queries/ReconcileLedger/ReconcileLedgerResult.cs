namespace AtlasPay.Finance.Application.Ledger.Queries.ReconcileLedger;

public sealed record ReconcileLedgerResult(
    int TotalAccounts,
    int MismatchedAccounts,
    IReadOnlyList<LedgerReconciliationItem> Items);