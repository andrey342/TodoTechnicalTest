namespace TodoManagement.API.Application.Commands.TodoManagement.RemoveTodoList;

public class RemoveTodoListCommandValidator : BaseValidator<RemoveTodoListCommand>
{
    public RemoveTodoListCommandValidator(IServiceProvider sp) : base(sp)
    {
        ValidateGuid<TodoList>(
            cmd => cmd.Id,
            isRequired: true);
    }
}