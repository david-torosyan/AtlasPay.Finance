namespace AtlasPay.Finance.Application.Abstractions.Persistence;

public interface IInternalLedgerRepository
{
    Task<AccountLedgerState> GetByCodeAsync(
        string code,
        CancellationToken ct = default);
}