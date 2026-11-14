using AtlasPay.Finance.Domain.Ledger.Entities;

namespace AtlasPay.Finance.Application.Abstractions.Persistence;

public interface ILedgerAccountRepository
{
    Task AddAsync(
        LedgerAccount ledgerAccount,
        CancellationToken cancellationToken = default);
}