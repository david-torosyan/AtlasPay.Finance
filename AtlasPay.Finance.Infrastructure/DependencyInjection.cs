using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Infrastructure.Persistence;
using AtlasPay.Finance.Infrastructure.Persistence.Repositories;
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

        return services;
    }
}