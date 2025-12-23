namespace TodoManagement.API.Application.Commands.TodoManagement.GenerateTodoListReport;

/// <summary>
/// Command to generate a report file for a TodoList.
/// </summary>
public record GenerateTodoListReportCommand(Guid Id) : IdempotentCommand<bool>;