namespace Contracts.Events.Common;

/// <summary>
/// Base contract for all external integration events.
/// </summary>
public abstract record IntegrationEvent
{
    /// <summary>
    /// Unique identifier for the integration event.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp indicating when the event was created (in UTC).
    /// </summary>
    public DateTime Created { get; init; } = DateTime.UtcNow;
}