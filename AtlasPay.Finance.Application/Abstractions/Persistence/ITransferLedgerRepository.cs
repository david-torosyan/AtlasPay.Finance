namespace AtlasPay.Finance.Application.Abstractions.Persistence;

public interface ITransferLedgerRepository
{
    Task<AccountLedgerState> GetByAccountIdAsync(
        Guid accountId,
        CancellationToken ct = default);
}