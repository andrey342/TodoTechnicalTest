namespace TodoManagement.API.Application.Commands.TodoManagement.RemoveTodoList;

public record RemoveTodoListCommand(Guid Id) : ICommand<string>;