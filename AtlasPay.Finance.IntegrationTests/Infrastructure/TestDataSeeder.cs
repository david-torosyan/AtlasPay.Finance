
using AtlasPay.Finance.Domain.Accounts.Entities;
using AtlasPay.Finance.Domain.Accounts.ValueObjects;
using AtlasPay.Finance.Domain.Ledger.Entities;
using AtlasPay.Finance.Domain.Ledger.ValueObjects;
using AtlasPay.Finance.Infrastructure.Persistence;

namespace AtlasPay.Finance.IntegrationTests.Infrastructure;

public static class TestDataSeeder
{
    public static async Task<(Guid AnnaId, Guid BobId)> SeedAsync(
        FinanceDbContext db,
        CancellationToken ct = default)
    {
        // 1. Create banking accounts.
        var anna = Account.Create(
            Guid.NewGuid(),
            Currency.AMD);

        var bob = Account.Create(
            Guid.NewGuid(),
            Currency.AMD);

        anna.Activate();
        bob.Activate();

        // 2. Create customer liability ledger accounts.
        var annaLedger = LedgerAccount.Create(
            anna.Id,
            LedgerAccountType.Liability,
            Currency.AMD);

        var bobLedger = LedgerAccount.Create(
            bob.Id,
            LedgerAccountType.Liability,
            Currency.AMD);

        // 3. Create a bank-owned cash asset ledger account.
        var cashLedger = LedgerAccount.Create(
            null,
            LedgerAccountType.Asset,
            Currency.AMD);

        // 4. Create zero balances for customer ledger accounts.
        var annaBalance = AccountBalance.Create(
            annaLedger.Id,
            Currency.AMD);

        var bobBalance = AccountBalance.Create(
            bobLedger.Id,
            Currency.AMD);

        // 5. Simulate initial funding with balanced ledger entries.

        var annaFunding = LedgerTransaction.Create(
            Guid.NewGuid());

        annaFunding.AddPosting(
            cashLedger.Id,
            PostingDirection.Debit,
            new Money(10_000m, Currency.AMD));

        annaFunding.AddPosting(
            annaLedger.Id,
            PostingDirection.Credit,
            new Money(10_000m, Currency.AMD));

        annaFunding.Post();

        var bobFunding = LedgerTransaction.Create(
            Guid.NewGuid());

        bobFunding.AddPosting(
            cashLedger.Id,
            PostingDirection.Debit,
            new Money(5_000m, Currency.AMD));

        bobFunding.AddPosting(
            bobLedger.Id,
            PostingDirection.Credit,
            new Money(5_000m, Currency.AMD));

        bobFunding.Post();

        // 6. Update customer liability balances.
        annaBalance.Credit(
            new Money(10_000m, Currency.AMD));

        bobBalance.Credit(
            new Money(5_000m, Currency.AMD));

        // 7. Stage all entities for insertion.
        db.Accounts.AddRange(anna, bob);

        db.LedgerAccounts.AddRange(
            annaLedger,
            bobLedger,
            cashLedger);

        db.AccountBalances.AddRange(
            annaBalance,
            bobBalance);

        db.LedgerTransactions.AddRange(
            annaFunding,
            bobFunding);

        // 8. Persist all records atomically.
        await db.SaveChangesAsync(ct);

        return (anna.Id, bob.Id);
    }
}
