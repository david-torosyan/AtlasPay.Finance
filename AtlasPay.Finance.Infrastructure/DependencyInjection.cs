using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Application.Ledger.Queries.CheckLedgerIntegrity;
using AtlasPay.Finance.Application.Ledger.Queries.ReconcileLedger;
using AtlasPay.Finance.Infrastructure.Persistence;
using AtlasPay.Finance.Infrastructure.Persistence.Repositories;
using AtlasPay.Finance.Infrastructure.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AtlasPay.Finance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration
            .GetConnectionString("FinanceDatabase")
            ?? throw new InvalidOperationException(
                "FinanceDatabase connection string is missing.");

        services.AddDbContext<FinanceDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<FinanceDbContext>());

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IAccountReadRepository, AccountReadRepository>();
        services.AddScoped<ILedgerAccountRepository, LedgerAccountRepository>();
        services.AddScoped<IAccountBalanceRepository, AccountBalanceRepository>();
        services.AddScoped<ITransferRepository, TransferRepository>();
        services.AddScoped<IAccountLockRepository, AccountLockRepository>();
        services.AddScoped<ITransferLedgerRepository, TransferLedgerRepository>();
        services.AddScoped<ILedgerTransactionRepository, LedgerTransactionRepository>();
        services.AddScoped<ITransferReadRepository, TransferReadRepository>();
        services.AddScoped<IDepositRepository, DepositRepository>();
        services.AddScoped<IInternalLedgerRepository, InternalLedgerRepository>();
        services.AddScoped<IAtomicBalanceRepository, AtomicBalanceRepository>();
        services.AddScoped<IWithdrawalRepository, WithdrawalRepository>();
        services.AddScoped<ITransferReversalRepository, TransferReversalRepository>();
        services.AddScoped<ILedgerReconciliationRepository, LedgerReconciliationRepository>();
        services.AddScoped<ILedgerIntegrityRepository, LedgerIntegrityRepository>();

        services.AddScoped<InternalLedgerBootstrapper>();

        return services;
    }
}