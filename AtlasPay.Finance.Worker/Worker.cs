
using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Worker.Messaging;

namespace AtlasPay.Finance.Worker;

public sealed class Worker(
    ILogger<Worker> logger,
    IServiceScopeFactory scopeFactory,
    IEventPublisher publisher) : BackgroundService
{
    private readonly string _workerId =
        $"finance-worker-{Guid.NewGuid():N}";

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Outbox Worker {WorkerId} started.",
            _workerId);

        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(5));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ProcessBatchAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Outbox Worker stopped.");
        }
    }

    private async Task ProcessBatchAsync(
       CancellationToken ct)
    {
        await using var scope =
            scopeFactory.CreateAsyncScope();

        var repository = scope.ServiceProvider
            .GetRequiredService<IOutboxRepository>();

        var claimedIds = await repository.ClaimBatchAsync(
           _workerId,
           batchSize: 1,
           leaseDuration: TimeSpan.FromSeconds(30),
           ct: ct);

        var messages = await repository.GetClaimedAsync(
            _workerId,
            claimedIds,
            ct);
        foreach (var message in messages)
        {
            try
            {
                await publisher.PublishAsync(
                    message.Id,
                    message.EventType,
                    message.Payload,
                    ct);

                var marked = await repository.MarkPublishedAsync(
                    message.Id,
                    _workerId,
                    ct);

                if (!marked)
                {
                    logger.LogWarning(
                        "Event {EventId} was published, but its " +
                        "lease was no longer valid.",
                        message.Id);

                    continue;
                }

                logger.LogInformation(
                    "Successfully published event {EventId}",
                    message.Id);
            }
            catch (OperationCanceledException)
                when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to publish event {EventId}",
                    message.Id);

                try
                {
                    var scheduled = await repository.MarkFailedAsync(
                        message.Id,
                        _workerId,
                        ex.Message,
                        ct);

                    if (!scheduled)
                    {
                        logger.LogWarning(
                            "Could not schedule retry for event {EventId}; " +
                            "lease may have expired.",
                            message.Id);
                    }
                }
                catch (Exception retryEx)
                {
                    logger.LogError(
                        retryEx,
                        "Failed to update retry state for event {EventId}",
                        message.Id);
                }
            }
        }
    }
}
