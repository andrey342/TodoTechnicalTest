namespace TodoManagement.API.Application.Commands.TodoManagement.RemoveTodoList;

/// <summary>
/// Command to remove an existing TodoList.
/// </summary>
public record RemoveTodoListCommand(Guid Id) : ICommand<string>;