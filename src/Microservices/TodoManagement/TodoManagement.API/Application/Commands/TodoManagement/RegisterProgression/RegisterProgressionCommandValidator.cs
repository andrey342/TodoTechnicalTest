namespace TodoManagement.API.Application.Commands.TodoManagement.RegisterProgression;

/// <summary>
/// Validator for RegisterProgressionCommand.
/// Validates that the TodoListId exists, ItemId is positive and is unique, Percent is valid, and ActionDate is required.
/// </summary>
public class RegisterProgressionCommandValidator : BaseValidator<RegisterProgressionCommand>
{
    public RegisterProgressionCommandValidator(IServiceProvider sp) : base(sp)
    {
        // Validate TodoListId: required and must exist in database
        ValidateGuid<TodoList>(
            cmd => cmd.TodoListId,
            isRequired: true);

        // Validate ItemId: required and positive
        ValidatePositiveNumber(
            cmd => cmd.ItemId,
            isRequired: true);

        // Validate TodoItem exists in database using composite index (TodoListId, ItemId)
        ValidateExists<TodoItem, RegisterProgressionCommand>(
            cmd => cmd,
            cmd => entity => entity.TodoListId == cmd.TodoListId && entity.ItemId == cmd.ItemId,
            "TodoItem does not exist.");

        // Validate Percent: required and must be between 0 (exclusive) and 100 (inclusive)
        ValidateDecimal(
            cmd => cmd.Progression.Percent,
            isRequired: true,
            precision: 5,
            scale: 2);

        // Additional rule: Percent must be > 0 and <= 100
        RuleFor(cmd => cmd.Progression.Percent)
            .GreaterThan(0)
            .WithMessage("Percent must be greater than 0.")
            .LessThanOrEqualTo(100)
            .WithMessage("Percent must not exceed 100.");
    }
}
