namespace TodoManagement.Domain.AggregatesModel.TodoListAggregate;

/// <summary>Todo item within the list</summary>
/// <remarks>Stores title, description, category (as string), and holds a progression history; IsCompleted is computed in domain</remarks>
public class TodoItem : Entity
{
    /// <summary>Foreign key to the owning TodoList</summary>
    public Guid TodoListId { get; private set; }
    /// <summary>Business identifier used by the repository (sequential int)</summary>
    public int ItemId { get; private set; }
    /// <summary>Item title</summary>
    public string Title { get; private set; } = string.Empty;
    /// <summary>Item description</summary>
    public string Description { get; private set; } = string.Empty;
    /// <summary>Category as a plain string (validated against system categories via repository)</summary>
    public string Category { get; private set; } = string.Empty;
    private readonly List<Progression> _progressions = new();
    public IReadOnlyCollection<Progression> Progressions => _progressions.AsReadOnly();
    private TodoItem() { }

    public TodoItem(
        Guid todolistid,
        int itemid,
        string title,
        string description,
        string category)
    {
        TodoListId = todolistid;
        ItemId = itemid;
        Title = title;
        Description = description;
        Category = category;
    }

    internal void UpdateTodoListId(Guid todolistid)
    {
        TodoListId = todolistid;
    }

    internal void UpdateItemId(int itemid)
    {
        ItemId = itemid;
    }

    internal void UpdateTitle(string title)
    {
        Title = title;
    }

    internal void UpdateDescription(string description)
    {
        Description = description;
    }

    internal void UpdateCategory(string category)
    {
        Category = category;
    }
}
