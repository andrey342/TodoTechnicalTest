namespace TodoManagement.Domain.AggregatesModel.TodoListAggregate;

/// <summary>
/// Interface that defines operations for managing TodoItems within a TodoList aggregate.
/// </summary>
public interface ITodoList
{
    /// <summary>
    /// Adds a new TodoItem to the list.
    /// </summary>
    /// <param name="id">The business identifier for the new item.</param>
    /// <param name="title">The title of the item.</param>
    /// <param name="description">The description of the item.</param>
    /// <param name="category">The category of the item.</param>
    void AddItem(int id, string title, string description, string category);

    /// <summary>
    /// Updates the description of an existing TodoItem.
    /// </summary>
    /// <param name="id">The business identifier of the item to update.</param>
    /// <param name="description">The new description.</param>
    void UpdateItem(int id, string description);

    /// <summary>
    /// Removes a TodoItem from the list, subject to business rules.
    /// </summary>
    /// <param name="id">The business identifier of the item to remove.</param>
    void RemoveItem(int id);

    /// <summary>
    /// Registers a new progression entry for a TodoItem.
    /// </summary>
    /// <param name="id">The business identifier of the item.</param>
    /// <param name="dateTime">The date when the progression was registered.</param>
    /// <param name="percent">The incremental percentage of progress.</param>
    void RegisterProgression(int id, DateTime dateTime, decimal percent);

    /// <summary>
    /// Prints all TodoItems with their progress information in a formatted output.
    /// </summary>
    void PrintItems();
}