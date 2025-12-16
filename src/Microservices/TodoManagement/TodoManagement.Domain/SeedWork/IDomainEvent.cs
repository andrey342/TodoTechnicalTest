namespace TodoManagement.Domain.SeedWork;

/// <summary>
/// Represents a domain event that can be published within the domain layer.
/// Implements the INotification interface to support integration with Mediator.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Gets the date and time when the domain event occurred.
    /// </summary>
    DateTime OccurredOn { get; }
}