using AtlasPay.Finance.Domain.Ledger.Entities;

namespace AtlasPay.Finance.Application.Abstractions.Persistence;

public sealed record AccountLedgerState(
    LedgerAccount LedgerAccount,
    AccountBalance Balance);