namespace TodoManagement.Domain.AggregatesModel.TodoListAggregate;

/// <summary>Todo list root entity</summary>
/// <remarks>Root of the aggregate; contains the collection of TodoItems</remarks>
public class TodoList : Entity, IAggregateRoot
{
    /// <summary>List name</summary>
    public string Name { get; private set; } = string.Empty;
    /// <summary>Last issued public numeric Id (supports repository GetNextId)</summary>
    public int LastIssuedPublicId { get; private set; } = 0;
    /// <summary>RowVersion for concurrency when issuing IDs / updating the list</summary>
    public byte[] RowVersion { get; private set; } = default!;
    private readonly List<TodoItem> _items = new();
    public IReadOnlyCollection<TodoItem> Items => _items.AsReadOnly();
    private TodoList() { }

    public TodoList(
        string name,
        int lastissuedpublicid)
    {
        ValidateDomainInvariants(name, lastissuedpublicid);
        Name = name;
        LastIssuedPublicId = lastissuedpublicid;
    }

    #region TodoItems behavior

    public void AddTodoItem(TodoItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        if (_items.Contains(item)) return;

        _items.Add(item);
    }

    public void RemoveTodoItem(TodoItem item)
    {
        if (item == null) return;
        _items.Remove(item);
    }

    #endregion

    #region Properties behavior

    public void UpdateName(string name)
    {
        ValidateName(name);
        Name = name;
    }

    public void UpdateLastIssuedPublicId(int lastissuedpublicid)
    {
        ValidateLastIssuedPublicId(lastissuedpublicid);
        LastIssuedPublicId = lastissuedpublicid;
    }

    #endregion

    #region Domain rules
    private const int NameMaxLength = 200;

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required", nameof(name));
        }
        if (name.Length > NameMaxLength)
        {
            throw new ArgumentException("Name exceeds maximum length of " + NameMaxLength, nameof(name));
        }
    }

    private static void ValidateLastIssuedPublicId(int lastissuedpublicid)
    {
        if (lastissuedpublicid < 0)
        {
            throw new ArgumentException("LastIssuedPublicId must be non-negative", nameof(lastissuedpublicid));
        }
    }

    private static void ValidateDomainInvariants(
        string name,
        int lastissuedpublicid)
    {
        ValidateName(name);
        ValidateLastIssuedPublicId(lastissuedpublicid);
    }

    #endregion
}
