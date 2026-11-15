
using AtlasPay.Finance.Domain.Accounts.ValueObjects;
using AtlasPay.Finance.Domain.Ledger.Entities;
using AtlasPay.Finance.Domain.Ledger.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Finance.Infrastructure.Persistence.Seeders;

public sealed class InternalLedgerBootstrapper(
    FinanceDbContext dbContext)
{
    public async Task InitializeAsync(
        CancellationToken ct = default)
    {
        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(ct);

        // Serialize concurrent bootstrap executions.
        await dbContext.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(1301326)",
            ct);

        var accounts = new[]
        {
            (LedgerAccountCode.CashAmd, Currency.AMD),
            (LedgerAccountCode.CashUsd, Currency.USD),
            (LedgerAccountCode.CashEur, Currency.EUR)
        };

        foreach (var (code, currency) in accounts)
        {
            var existing = await dbContext.LedgerAccounts
                .SingleOrDefaultAsync(
                    x => x.Code == code, ct);

            if (existing is not null)
            {
                if (existing.Type != LedgerAccountType.Asset ||
                    existing.Currency != currency ||
                    existing.AccountId is not null)
                {
                    throw new InvalidOperationException(
                        $"Internal ledger account {code} is misconfigured.");
                }

                var balanceExists = await dbContext.AccountBalances
                    .AnyAsync(
                        x => x.LedgerAccountId == existing.Id,
                        ct);

                if (!balanceExists)
                    throw new InvalidOperationException(
                        $"Balance missing for {code}.");

                continue;
            }

            var ledgerAccount = LedgerAccount.CreateInternal(
                code,
                LedgerAccountType.Asset,
                currency);

            var balance = AccountBalance.Create(
                ledgerAccount.Id,
                currency);

            await dbContext.LedgerAccounts.AddAsync(
                ledgerAccount, ct);

            await dbContext.AccountBalances.AddAsync(
                balance, ct);
        }

        await dbContext.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);
    }
}
