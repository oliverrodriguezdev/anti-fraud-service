using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class KafkaEventConsumer
{
    private readonly IConfiguration _configuration;
    private readonly string _topic;
    private readonly ILogger<KafkaEventConsumer> _logger;

    public KafkaEventConsumer(IConfiguration configuration, string topic, ILogger<KafkaEventConsumer> logger)
    {
        _configuration = configuration;
        _topic = topic;
        _logger = logger;
    }

    public void Consume(Action<string> handleMessage, CancellationToken cancellationToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"],
            GroupId = "antifraud-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _logger.LogInformation("Starting Kafka consumer for topic {Topic} with BootstrapServers: {BootstrapServers}", 
            _topic, config.BootstrapServers);

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(_topic);

        _logger.LogInformation("Subscribed to topic: {Topic}", _topic);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug("Waiting for message from topic: {Topic}", _topic);
                var result = consumer.Consume(cancellationToken);
                _logger.LogInformation("Received message from topic {Topic}: {Message}", _topic, result.Message.Value);
                handleMessage(result.Message.Value);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Kafka consumer cancelled for topic: {Topic}", _topic);
            consumer.Close();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Kafka consumer for topic: {Topic}", _topic);
            throw;
        }
    }
}