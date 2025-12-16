namespace TodoManagement.Domain.IRepositories;

/// <summary>
/// Repository interface for command operations.
/// Implements default command methods provided by ICommandRepository&lt;T&gt;.
/// </summary>
public interface ITodoListRepository : ICommandRepository<TodoList>
{
    
}
