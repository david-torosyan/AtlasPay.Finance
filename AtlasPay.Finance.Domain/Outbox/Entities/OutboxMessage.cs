namespace AtlasPay.Finance.Domain.Outbox.Entities;

public sealed class OutboxMessage
{
    private OutboxMessage() { } // For EF Core

    public Guid Id { get; private set; }

    public string EventType { get; private set; } = null!;

    public string Payload { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }

    public int Attempts { get; private set; }

    public DateTimeOffset NextAttemptAt { get; private set; }

    public DateTimeOffset? LockedUntil { get; private set; }

    public string? LockedBy { get; private set; }

    public string? LastError { get; private set; }

    public static OutboxMessage Create(
        Guid eventId,
        string eventType,
        string payload)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException(
                "Event ID cannot be empty.",
                nameof(eventId));

        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        var now = DateTimeOffset.UtcNow;

        return new OutboxMessage
        {
            Id = eventId,
            EventType = eventType,
            Payload = payload,
            CreatedAt = now,
            NextAttemptAt = now,
            Attempts = 0
        };
    }
}