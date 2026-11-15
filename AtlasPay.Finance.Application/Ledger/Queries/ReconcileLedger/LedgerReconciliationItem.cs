namespace AtlasPay.Finance.Application.Ledger.Queries.ReconcileLedger;

public sealed record LedgerReconciliationItem(
    Guid LedgerAccountId,
    string? Code,
    string AccountType,
    string Currency,
    decimal ExpectedBalance,
    decimal? StoredBalance,
    decimal? Difference,
    bool IsMatch);