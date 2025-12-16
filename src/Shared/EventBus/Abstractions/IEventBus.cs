namespace EventBus.Abstractions;

/// <summary>
/// Defines the contract for an event bus that handles publishing integration events
/// across microservices using the publish-subscribe pattern.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an integration event using the default topic naming convention.
    /// The topic name is automatically derived from the event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of integration event to publish</typeparam>
    /// <param name="event">The event instance to publish</param>
    /// <param name="ct">Cancellation token to cancel the operation</param>
    /// <returns>A task representing the asynchronous publish operation</returns>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : IntegrationEvent;

    /// <summary>
    /// Publishes an integration event to a specific topic.
    /// This allows for explicit control over the target topic name.
    /// </summary>
    /// <typeparam name="TEvent">The type of integration event to publish</typeparam>
    /// <param name="topic">The specific topic name to publish to</param>
    /// <param name="event">The event instance to publish</param>
    /// <param name="ct">Cancellation token to cancel the operation</param>
    /// <returns>A task representing the asynchronous publish operation</returns>
    Task PublishAsync<TEvent>(string topic, TEvent @event, CancellationToken ct = default)
        where TEvent : IntegrationEvent;
}