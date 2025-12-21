namespace TodoManagement.API.Application.Commands.TodoManagement.RemoveTodoItem;

/// <summary>
/// Validator for RemoveTodoItemCommand.
/// Validates that the TodoListId exists and ItemId is positive and exists.
/// </summary>
public class RemoveTodoItemCommandValidator : BaseValidator<RemoveTodoItemCommand>
{
    public RemoveTodoItemCommandValidator(IServiceProvider sp) : base(sp)
    {
        // Validate TodoListId: required and must exist in database
        ValidateGuid<TodoList>(
            cmd => cmd.TodoItem.TodoListId,
            isRequired: true);

        // Validate ItemId: required and positive
        ValidatePositiveNumber(
            cmd => cmd.TodoItem.ItemId,
            isRequired: true);

        // Validate TodoItem exists in database using composite index (TodoListId, ItemId)
        ValidateExists<TodoItem, RemoveTodoItemDto>(
            cmd => cmd.TodoItem,
            dto => entity => entity.TodoListId == dto.TodoListId && entity.ItemId == dto.ItemId,
            "TodoItem does not exist.");
    }
}
