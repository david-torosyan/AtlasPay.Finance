using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Domain.Accounts.Entities;
using AtlasPay.Finance.Domain.Ledger.Entities;
using AtlasPay.Finance.Domain.Transfers.Entities;
using Microsoft.EntityFrameworkCore;

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
