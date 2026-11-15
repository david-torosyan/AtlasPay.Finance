using AtlasPay.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Finance.Worker;

public sealed class Worker(
    ILogger<Worker> logger,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox Worker started.");

        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(5));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await using var scope = scopeFactory.CreateAsyncScope();

                var dbContext = scope.ServiceProvider
                    .GetRequiredService<FinanceDbContext>();

                var pendingCount = await dbContext.OutboxMessages
                    .AsNoTracking()
                    .CountAsync(
                        x => x.PublishedAt == null,
                        stoppingToken);

                logger.LogInformation(
                    "Pending outbox messages: {Count}",
                    pendingCount);
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Outbox Worker stopped.");
        }
    }
}