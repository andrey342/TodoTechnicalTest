namespace TodoManagement.API.Application.Commands.TodoManagement.AddTodoItem;

/// <summary>
/// Validator for AddTodoItemCommand.
/// Validates that the TodoListId exists, and that Title, Description, and Category are required with appropriate maximum lengths.
/// </summary>
public class AddTodoItemCommandValidator : BaseValidator<AddTodoItemCommand>
{
    public AddTodoItemCommandValidator(IServiceProvider sp) : base(sp)
    {
        // Validate TodoListId: required and must exist in database
        ValidateGuid<TodoList>(
            cmd => cmd.TodoItem.TodoListId,
            isRequired: true);

        // Validate Title: required, max 200 characters
        ValidateString(
            cmd => cmd.TodoItem.Title,
            maxLength: 200,
            isRequired: true);

        // Validate Description: required, max 1000 characters
        ValidateString(
            cmd => cmd.TodoItem.Description,
            maxLength: 1000,
            isRequired: true);

        // Validate Category: required, max 100 characters
        ValidateString(
            cmd => cmd.TodoItem.Category,
            maxLength: 100,
            isRequired: true);
    }
}