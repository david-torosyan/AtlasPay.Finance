using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Domain.Ledger.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Finance.Infrastructure.Persistence.Repositories;

public sealed class TransferLedgerRepository(
    FinanceDbContext dbContext)
    : ITransferLedgerRepository
{
    public async Task<AccountLedgerState> GetByAccountIdAsync(
        Guid accountId,
        CancellationToken ct = default)
    {
        var ledgerAccount = await dbContext.LedgerAccounts
            .AsTracking()
            .SingleOrDefaultAsync(
                x => x.AccountId == accountId &&
                     x.Type == LedgerAccountType.Liability,
                ct);

        if (ledgerAccount is null)
            throw new InvalidOperationException(
                $"Liability ledger account not found for {accountId}.");

        var balance = await dbContext.AccountBalances
            .AsTracking()
            .SingleOrDefaultAsync(
                x => x.LedgerAccountId == ledgerAccount.Id,
                ct);

        if (balance is null)
            throw new InvalidOperationException(
                $"Balance not found for ledger account {ledgerAccount.Id}.");

        return new AccountLedgerState(
            ledgerAccount,
            balance);
    }
}