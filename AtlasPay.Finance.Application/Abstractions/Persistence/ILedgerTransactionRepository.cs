using AtlasPay.Finance.Domain.Ledger.Entities;

namespace AtlasPay.Finance.Application.Abstractions.Persistence;

public interface ILedgerTransactionRepository
{
    Task AddAsync(
        LedgerTransaction transaction,
        CancellationToken ct = default);
}