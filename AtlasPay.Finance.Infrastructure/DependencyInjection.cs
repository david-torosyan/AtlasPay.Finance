using AtlasPay.Finance.Infrastructure.Persistence;
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

        return services;
    }
}