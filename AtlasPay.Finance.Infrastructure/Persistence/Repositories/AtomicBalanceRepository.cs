
using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Domain.Ledger.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Finance.Infrastructure.Persistence.Repositories;

public sealed class AtomicBalanceRepository(
    FinanceDbContext dbContext) : IAtomicBalanceRepository
{
    public async Task IncreaseAsync(
        Guid ledgerAccountId,
        Money amount,
        CancellationToken ct = default)
    {
        if (ledgerAccountId == Guid.Empty)
            throw new ArgumentException(
                "Ledger account ID cannot be empty.",
                nameof(ledgerAccountId));

        ArgumentNullException.ThrowIfNull(amount);

        if (amount.Amount <= 0)
            throw new ArgumentException(
                "Amount must be positive.",
                nameof(amount));

        var affectedRows = await dbContext.Database
            .ExecuteSqlInterpolatedAsync(
                $"""
                UPDATE account_balances
                SET
                    "balance" = "balance" + {amount.Amount},
                    "UpdatedAt" = NOW()
                WHERE "LedgerAccountId" = {ledgerAccountId}
                  AND "currency" = {amount.Currency.ToString()}
                """,
                ct);

        if (affectedRows != 1)
            throw new InvalidOperationException(
                "Atomic balance update failed.");
    }

    public async Task<bool> TryDecreaseAsync(
        Guid ledgerAccountId,
        Money amount,
        CancellationToken ct = default)
    {
        if (ledgerAccountId == Guid.Empty)
            throw new ArgumentException(
                "Ledger account ID cannot be empty.",
                nameof(ledgerAccountId));

        ArgumentNullException.ThrowIfNull(amount);

        if (amount.Amount <= 0)
            throw new ArgumentException(
                "Amount must be positive.",
                nameof(amount));

        var affectedRows = await dbContext.Database
            .ExecuteSqlInterpolatedAsync(
                $"""
                UPDATE account_balances
                SET
                    "balance" = "balance" - {amount.Amount},
                    "UpdatedAt" = NOW()
                WHERE "LedgerAccountId" = {ledgerAccountId}
                  AND "currency" = {amount.Currency.ToString()}
                  AND "balance" >= {amount.Amount}
                """,
                ct);

        if (affectedRows == 1)
            return true;


        var currentBalance = await dbContext.AccountBalances
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.LedgerAccountId == ledgerAccountId,
                ct);

        if (currentBalance is null)
            throw new InvalidOperationException(
                "Ledger account balance was not found.");

        if (currentBalance.Balance.Currency != amount.Currency)
            throw new InvalidOperationException(
                "Ledger account currency mismatch.");

        return false;
    }
}
