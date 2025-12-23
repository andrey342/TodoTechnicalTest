namespace TodoManagement.Domain.Events;

/// <summary>
/// Domain event raised when TodoList items are printed.
/// Contains the generated report content for integration event publishing.
/// </summary>
public record ItemsPrintedDomainEvent(
    Guid TodoListId,
    string TodoListName,
    string Content) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}