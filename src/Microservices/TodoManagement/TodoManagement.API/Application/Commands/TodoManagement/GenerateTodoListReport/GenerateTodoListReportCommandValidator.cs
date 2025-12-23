namespace TodoManagement.API.Application.Commands.TodoManagement.GenerateTodoListReport;

public class GenerateTodoListReportCommandValidator : BaseValidator<GenerateTodoListReportCommand>
{
    public GenerateTodoListReportCommandValidator(IServiceProvider sp) : base(sp)
    {
        ValidateGuid<TodoList>(
            cmd => cmd.Id,
            isRequired: true);
    }
}