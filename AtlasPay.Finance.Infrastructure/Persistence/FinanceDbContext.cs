using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Application.Deposits.Exceptions;
using AtlasPay.Finance.Application.Transfers.Exceptions;
using AtlasPay.Finance.Domain.Accounts.Entities;
using AtlasPay.Finance.Domain.Deposits.Entities;
using AtlasPay.Finance.Domain.Ledger.Entities;
using AtlasPay.Finance.Domain.Transfers.Entities;
using AtlasPay.Finance.Domain.Withdrawals.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AtlasPay.Finance.Infrastructure.Persistence;

public sealed class FinanceDbContext : DbContext, IUnitOfWork
{
    public FinanceDbContext(
        DbContextOptions<FinanceDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts
        => Set<Account>();
    public DbSet<LedgerAccount> LedgerAccounts
       => Set<LedgerAccount>();
    public DbSet<LedgerTransaction> LedgerTransactions
        => Set<LedgerTransaction>();
    public DbSet<LedgerPosting> LedgerPostings
        => Set<LedgerPosting>();
    public DbSet<AccountBalance> AccountBalances
        => Set<AccountBalance>();
    public DbSet<Transfer> Transfers
        => Set<Transfer>();
    public DbSet<Deposit> Deposits
        => Set<Deposit>();
    public DbSet<Withdrawal> Withdrawals
        => Set<Withdrawal>();
    public DbSet<TransferReversal> TransferReversals
        => Set<TransferReversal>();

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await using var transaction = await Database.BeginTransactionAsync(ct);

        try
        {
            await operation(ct);

            await SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException pg &&
                  pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            await transaction.RollbackAsync(CancellationToken.None);

            var postgresException = (PostgresException)ex.InnerException!;

            switch (postgresException.ConstraintName)
            {
                case "IX_transfers_RequestId":
                    throw new DuplicateTransferRequestException();

                case "IX_deposits_RequestId":
                    throw new DuplicateDepositRequestException();

                case "IX_withdrawals_RequestId":
                    throw new DuplicateWithdrawalRequestException();

                case "IX_transfer_reversals_RequestId":
                    throw new DuplicateTransferReversalRequestException();

                case "IX_transfer_reversals_OriginalTransferId":
                    throw new TransferReversalConflictException(
                        "This transfer has already been reversed.");

                default:
                    throw;
            }
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(FinanceDbContext).Assembly);
    }
}
