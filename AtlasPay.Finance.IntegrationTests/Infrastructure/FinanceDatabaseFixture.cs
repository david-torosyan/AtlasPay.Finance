using AtlasPay.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Finance.IntegrationTests.Infrastructure;

public sealed class FinanceDatabaseFixture : IAsyncLifetime
{
    public FinanceDatabaseFixture()
    {
        var builder =
            new Npgsql.NpgsqlConnectionStringBuilder(ConnectionString._connectionString);

        if (builder.Database != "atlaspay_finance_tests")
            throw new InvalidOperationException(
                "Integration tests must use atlaspay_finance_tests.");
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseNpgsql(ConnectionString._connectionString)
            .Options;

        await using var db = new FinanceDbContext(options);

        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}