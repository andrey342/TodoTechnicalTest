namespace EventBus.Cap;

/// <summary>
/// Implementation of the event bus using the CAP framework for reliable message publishing.
/// Supports automatic topic naming, payload compression for large events, and flexible publishing options.
/// </summary>
public sealed class CapEventBus : IEventBus
{
    #region Fields and Constructor

    /// <summary>
    /// Maximum payload size in bytes before compression is applied (0.9 MB).
    /// Events larger than this threshold will be compressed to reduce network overhead.
    /// </summary>
    private const int MaxBytes = 900_000; // 0.9 MB

    private readonly ICapPublisher _cap;
    private readonly IEventSerializer _ser;
    private readonly ILogger<CapEventBus> _log;

    /// <summary>
    /// Initializes a new instance of the CapEventBus.
    /// </summary>
    /// <param name="cap">The CAP publisher for message publishing</param>
    /// <param name="ser">The event serializer for payload processing</param>
    /// <param name="log">The logger for tracking publishing operations</param>
    public CapEventBus(ICapPublisher cap, IEventSerializer ser, ILogger<CapEventBus> log)
        => (_cap, _ser, _log) = (cap, ser, log);

    #endregion

    #region IEventBus Implementation

    /// <inheritdoc/>
    public Task PublishAsync<TEvent>(TEvent evt, CancellationToken ct = default)
        where TEvent : IntegrationEvent =>
        PublishCoreAsync(GetTopicName<TEvent>(), evt, ct);

    /// <inheritdoc/>
    public Task PublishAsync<TEvent>(string topic, TEvent evt, CancellationToken ct = default)
        where TEvent : IntegrationEvent =>
        PublishCoreAsync($"integration.{topic.ToLowerInvariant()}", evt, ct);

    #endregion

    #region Private Implementation

    /// <summary>
    /// Core publishing logic that handles serialization, compression decisions, and actual message publishing.
    /// Automatically determines whether to send the event as a raw object or compressed bytes based on size.
    /// </summary>
    /// <typeparam name="TEvent">The type of integration event to publish</typeparam>
    /// <param name="topic">The target topic name</param>
    /// <param name="evt">The event instance to publish</param>
    /// <param name="ct">Cancellation token to cancel the operation</param>
    /// <returns>A task representing the asynchronous publish operation</returns>
    private async Task PublishCoreAsync<TEvent>(string topic, TEvent evt, CancellationToken ct)
        where TEvent : IntegrationEvent
    {
        // Serialize the event to determine its size
        var bytes = _ser.Serialize(evt);

        // Determine delivery mode based on payload size
        // Small payloads: sent as raw objects for better performance
        // Large payloads: sent as compressed byte arrays to reduce network overhead
        var suffix = bytes.Length < MaxBytes ? ".raw" : ".gz";
        var payload = bytes.Length < MaxBytes ? evt : (object)bytes;

        _log.LogInformation("Publishing {Event} to {Topic} as {Mode} ({Size} B)",
                            typeof(TEvent).Name, topic + suffix,
                            payload is byte[]? "BYTE[]" : "OBJECT", bytes.Length);

        await _cap.PublishAsync(topic + suffix, payload, cancellationToken: ct);
    }

    /// <summary>
    /// Generates a standardized topic name from the event type.
    /// Removes the "Event" suffix from the type name and applies lowercase formatting.
    /// </summary>
    /// <typeparam name="T">The event type to generate a topic name for</typeparam>
    /// <returns>A formatted topic name in the format "integration.{eventname}"</returns>
    private static string GetTopicName<T>() =>
        $"integration.{typeof(T).Name.TrimEnd("Event".ToCharArray()).ToLowerInvariant()}";

    #endregion
}