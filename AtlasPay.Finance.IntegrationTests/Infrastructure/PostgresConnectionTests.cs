using Npgsql;

namespace AtlasPay.Finance.IntegrationTests.Infrastructure;

public sealed class PostgresConnectionTests
{
    [Fact]
    public async Task PostgreSql_Should_Be_Reachable()
    {
        var builder = new NpgsqlConnectionStringBuilder(
            ConnectionString._connectionString);

        Assert.Equal("atlaspay_finance_tests", builder.Database);

        await using var connection =
            new NpgsqlConnection(ConnectionString._connectionString);

        await connection.OpenAsync();

        Assert.Equal(
            System.Data.ConnectionState.Open,
            connection.State);
    }
}