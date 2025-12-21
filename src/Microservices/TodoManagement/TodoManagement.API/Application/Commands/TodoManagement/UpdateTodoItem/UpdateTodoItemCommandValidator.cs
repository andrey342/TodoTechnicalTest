namespace TodoManagement.API.Application.Commands.TodoManagement.UpdateTodoItem;

/// <summary>
/// Validator for UpdateTodoItemCommand.
/// Validates that the TodoListId exists, ItemId is positive and exists in database (using composite index TodoListId, ItemId), and Description is required.
/// </summary>
public class UpdateTodoItemCommandValidator : BaseValidator<UpdateTodoItemCommand>
{
    public UpdateTodoItemCommandValidator(IServiceProvider sp) : base(sp)
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
        ValidateExists<TodoItem, UpdateTodoItemDto>(
            cmd => cmd.TodoItem,
            dto => entity => entity.TodoListId == dto.TodoListId && entity.ItemId == dto.ItemId,
            "TodoItem does not exist.");

        // Validate Description: required, max 1000 characters
        ValidateString(
            cmd => cmd.TodoItem.Description,
            maxLength: 1000,
            isRequired: true);
    }
}