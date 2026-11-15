
using Confluent.Kafka;
using System.Text;

namespace AtlasPay.Finance.Worker.Messaging;

public sealed class KafkaEventPublisher : IEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;

    public KafkaEventPublisher(IConfiguration configuration)
    {
        _topic = configuration["Kafka:Topic"]
            ?? throw new InvalidOperationException(
                "Kafka topic is not configured.");

        var bootstrapServers = configuration["Kafka:BootstrapServers"]
            ?? throw new InvalidOperationException(
                "Kafka bootstrap servers are not configured.");

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true
        };

        _producer = new ProducerBuilder<string, string>(config)
            .Build();
    }

    public async Task PublishAsync(
        Guid eventId,
        string eventType,
        string payload,
        CancellationToken ct = default)
    {
        var message = new Message<string, string>
        {
            Key = eventId.ToString("N"),
            Value = payload,
            Headers = new Headers
            {
                {
                    "event-id",
                    Encoding.UTF8.GetBytes(eventId.ToString())
                },
                {
                    "event-type",
                    Encoding.UTF8.GetBytes(eventType)
                }
            }
        };

        var result = await _producer.ProduceAsync(
            _topic,
            message,
            ct);

        if (result.Status != PersistenceStatus.Persisted)
        {
            throw new InvalidOperationException(
                $"Kafka did not confirm persistence for event {eventId}.");
        }
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}
