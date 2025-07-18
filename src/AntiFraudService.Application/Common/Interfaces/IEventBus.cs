namespace AntiFraudService.Application.Common.Interfaces;

public interface IEventBus
{
    Task PublishAsync<T>(string topic, T @event, CancellationToken cancellationToken = default);
}