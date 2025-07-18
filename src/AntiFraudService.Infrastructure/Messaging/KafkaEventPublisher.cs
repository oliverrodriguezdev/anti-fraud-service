using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AntiFraudService.Infrastructure.Messaging;

public class KafkaEventPublisher
{
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<KafkaEventPublisher> _logger;

    public KafkaEventPublisher(IConfiguration configuration, ILogger<KafkaEventPublisher> logger)
    {
        _logger = logger;
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"]
        };

        _logger.LogInformation("KafkaEventPublisher initialized with BootstrapServers: {BootstrapServers}", config.BootstrapServers);
        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task PublishAsync<T>(string topic, T @event, CancellationToken cancellationToken = default)
    {
        var message = JsonSerializer.Serialize(@event);
        _logger.LogInformation("Publishing message to topic {Topic}: {Message}", topic, message);
        
        try
        {
            var result = await _producer.ProduceAsync(topic, new Message<Null, string> { Value = message }, cancellationToken);
            _logger.LogInformation("Message published successfully to topic {Topic}, partition {Partition}, offset {Offset}", 
                topic, result.Partition, result.Offset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to topic {Topic}", topic);
            throw;
        }
    }
}
