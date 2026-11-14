using AtlasPay.Finance.Domain.Accounts.Entities;
using AtlasPay.Finance.Domain.Ledger.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Finance.Infrastructure.Persistence;

public sealed class FinanceDbContext : DbContext
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(FinanceDbContext).Assembly);
    }
}
