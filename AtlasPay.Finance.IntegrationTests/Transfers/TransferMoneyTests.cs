
using AtlasPay.Finance.Application.Transfers.Commands.TransferMoney;
using AtlasPay.Finance.Domain.Accounts.ValueObjects;
using AtlasPay.Finance.Domain.Ledger.ValueObjects;
using AtlasPay.Finance.Domain.Transfers.ValueObjects;
using AtlasPay.Finance.Infrastructure.Persistence;
using AtlasPay.Finance.Infrastructure.Persistence.Repositories;
using AtlasPay.Finance.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Finance.IntegrationTests.Transfers;

public sealed class TransferMoneyTests
    : IClassFixture<FinanceDatabaseFixture>
{
    private static FinanceDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseNpgsql(ConnectionString._connectionString)
            .Options;

        return new FinanceDbContext(options);
    }

    [Fact]
    public async Task Transfer_Should_Move_Money_And_Create_Two_Postings()
    {
        Guid annaId;
        Guid bobId;

        await using (var db = CreateDb())
        {
            (annaId, bobId) = await TestDataSeeder.SeedAsync(db);
        }

        var requestId = Guid.NewGuid();

        Guid transferId;

        await using (var db = CreateDb())
        {
            var handler = new TransferMoneyHandler(
                db,
                new AccountLockRepository(db),
                new TransferLedgerRepository(db),
                new TransferRepository(db),
                new LedgerTransactionRepository(db),
                new OutboxRepository(db));

            transferId = await handler.HandleAsync(
                new TransferMoneyCommand(
                    requestId,
                    annaId,
                    bobId,
                    3_000m,
                    Currency.AMD));
        }

        await using (var db = CreateDb())
        {
            var transfer = await db.Transfers.SingleAsync(
                x => x.Id == transferId);

            Assert.Equal(
                TransferStatus.Completed,
                transfer.Status);

            Assert.Equal(requestId, transfer.RequestId);

            var ledgerTransaction = await db.LedgerTransactions
                .SingleAsync(x => x.ReferenceId == transferId);

            Assert.True(ledgerTransaction.IsPosted);

            var postings = await db.LedgerPostings
                .Where(x =>
                    x.LedgerTransactionId == ledgerTransaction.Id)
                .ToListAsync();

            Assert.Equal(2, postings.Count);

            var annaLedger = await db.LedgerAccounts
                .SingleAsync(x => x.AccountId == annaId);

            var bobLedger = await db.LedgerAccounts
                .SingleAsync(x => x.AccountId == bobId);

            Assert.Contains(postings, p =>
                p.LedgerAccountId == annaLedger.Id &&
                p.Direction == PostingDirection.Debit &&
                p.Amount.Amount == 3_000m);

            Assert.Contains(postings, p =>
                p.LedgerAccountId == bobLedger.Id &&
                p.Direction == PostingDirection.Credit &&
                p.Amount.Amount == 3_000m);

            var annaBalance = await db.AccountBalances
                .SingleAsync(x =>
                    x.LedgerAccountId == annaLedger.Id);

            var bobBalance = await db.AccountBalances
                .SingleAsync(x =>
                    x.LedgerAccountId == bobLedger.Id);

            Assert.Equal(
                7_000m,
                annaBalance.Balance.Amount);

            Assert.Equal(
                8_000m,
                bobBalance.Balance.Amount);
        }
    }
}
