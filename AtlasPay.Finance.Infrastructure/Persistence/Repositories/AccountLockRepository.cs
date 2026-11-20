using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Domain.Accounts.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Finance.Infrastructure.Persistence.Repositories;

public sealed class AccountLockRepository(
    FinanceDbContext dbContext) : IAccountLockRepository
{
    public async Task<Account> LockAccountAsync(
        Guid accountId,
        Guid customerId,
        CancellationToken ct = default)
    {
        if (accountId == Guid.Empty)
            throw new ArgumentException(
                "Account ID cannot be empty.",
                nameof(accountId));

        var accounts = await dbContext.Accounts
                .FromSqlInterpolated(
                    $"""
                    SELECT * FROM accounts
                    WHERE "Id" = {accountId}
                    AND "CustomerId" = {customerId} FOR UPDATE
                    """)
                .AsTracking()
                .ToListAsync(ct);

        return accounts.SingleOrDefault()
        ?? throw new InvalidOperationException(
            "Account not found.");
    }

    public async Task<IReadOnlyList<Account>> LockAccountsAsync(
        Guid firstAccountId,
        Guid secondAccountId,
        CancellationToken ct = default)
    {
        if (firstAccountId == secondAccountId)
            throw new ArgumentException(
                "Accounts must be different.");

        var ids = new[] { firstAccountId, secondAccountId };
        Array.Sort(ids);

        var accounts = new List<Account>(2);

        foreach (var id in ids)
        {
            var matches = await dbContext.Accounts
                .FromSqlInterpolated(
                    $"""SELECT * FROM accounts WHERE "Id" = {id} FOR UPDATE""")
                .AsTracking()
                .ToListAsync(ct);

            if (matches.Count != 1)
                throw new InvalidOperationException(
                    $"Account {id} was not found.");

            accounts.Add(matches[0]);
        }

        return accounts;
    }
}