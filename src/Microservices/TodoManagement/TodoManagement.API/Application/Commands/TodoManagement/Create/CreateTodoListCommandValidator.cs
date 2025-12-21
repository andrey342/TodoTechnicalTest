namespace TodoManagement.API.Application.Commands.TodoManagement.Create;

/// <summary>
/// Validator for CreateTodoListCommand.
/// Validates that the TodoList name is required, has maximum length of 50 characters, and is unique in the database.
/// </summary>
public class CreateTodoListCommandValidator : BaseValidator<CreateTodoListCommand>
{
    public CreateTodoListCommandValidator(IServiceProvider sp) : base(sp)
    {
        // Validate Name: required, max 50 characters
        ValidateString(
            cmd => cmd.TodoList.Name,
            maxLength: 50,
            isRequired: true);

        // Validate Name uniqueness in database
        ValidateUniqueness<TodoList, string>(
            cmd => cmd.TodoList.Name,
            name => entity => entity.Name == name);
    }
}