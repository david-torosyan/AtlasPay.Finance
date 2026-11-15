using AtlasPay.Finance.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace AtlasPay.Finance.Infrastructure.Persistence.Repositories;

public sealed class InternalLedgerRepository(
    FinanceDbContext dbContext) : IInternalLedgerRepository
{
    public async Task<AccountLedgerState> GetByCodeAsync(
     string code,
     CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException(
                "Ledger account code is required.",
                nameof(code));

        var ledgerAccount = await dbContext.LedgerAccounts
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Code == code, ct)
            ?? throw new InvalidOperationException(
                $"Internal ledger account {code} not found.");

        var balance = await dbContext.AccountBalances
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.LedgerAccountId == ledgerAccount.Id,
                ct)
            ?? throw new InvalidOperationException(
                $"Balance for {code} not found.");

        return new AccountLedgerState(ledgerAccount, balance);
    }
}