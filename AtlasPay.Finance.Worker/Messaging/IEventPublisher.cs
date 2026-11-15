namespace AtlasPay.Finance.Worker.Messaging;

public interface IEventPublisher
{
    Task PublishAsync(
        Guid eventId,
        string eventType,
        string payload,
        CancellationToken ct = default);
}