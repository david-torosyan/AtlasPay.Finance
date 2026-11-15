namespace AtlasPay.Finance.Application.Ledger.Queries.CheckLedgerIntegrity;

public sealed record UnbalancedTransactionItem(
    Guid LedgerTransactionId,
    string Currency,
    decimal TotalDebit,
    decimal TotalCredit,
    decimal Difference);