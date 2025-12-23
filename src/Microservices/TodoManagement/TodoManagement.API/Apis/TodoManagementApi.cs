namespace TodoManagement.API.Apis;

using static TodoManagement.API.Apis.Common.Gateways.GatewayTarget;

public static class TodoManagementApi
{
    public static RouteGroupBuilder MapTodoManagementApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("todo-management");

        // Commands
        api.MapPost("/todoList", CreateTodoList)
            .WithMetadata(new IncludeInGatewayAttribute(
                targets: [ApiGateway] // If you want to specify targets, if not, the default is all targets
                ));

        api.MapDelete("/todoList", RemoveTodoList)
            .WithMetadata(new IncludeInGatewayAttribute());

        api.MapPost("/todoList/item", AddTodoItem)
            .WithMetadata(new IncludeInGatewayAttribute());

        api.MapPut("/todoList/item", UpdateTodoItem)
            .WithMetadata(new IncludeInGatewayAttribute());

        api.MapDelete("/todoList/item", RemoveTodoItem)
            .WithMetadata(new IncludeInGatewayAttribute());

        api.MapPost("/todoList/item/progression", RegisterProgression)
            .WithMetadata(new IncludeInGatewayAttribute());

        api.MapPost("/todoList/printItemsFile", GenerateTodoListReport)
            .WithMetadata(new IncludeInGatewayAttribute());

        // Queries
        api.MapGet("/todoList/{id:guid}", GetTodoListById)
            .WithMetadata(new IncludeInGatewayAttribute());

        api.MapGet("/todoList", GetAllTodoLists)
            .WithMetadata(new IncludeInGatewayAttribute());

        api.MapGet("/categories", GetAllCategories)
            .WithMetadata(new IncludeInGatewayAttribute());

        return api;
    }

    #region Commands

    private static async Task<Ok<string>> CreateTodoList(
        CreateTodoListCommand command,
        [AsParameters] TodoManagementServices services)
    {
        var res = await services.Mediator.Send(command);

        return TypedResults.Ok(res);
    }

    private static async Task<Ok<string>> RemoveTodoList(
        [FromBody] RemoveTodoListCommand command,
        [AsParameters] TodoManagementServices services)
    {
        var res = await services.Mediator.Send(command);

        return TypedResults.Ok(res);
    }

    private static async Task<Ok<string>> AddTodoItem(
        AddTodoItemCommand command,
        [AsParameters] TodoManagementServices services)
    {
        var res = await services.Mediator.Send(command);

        return TypedResults.Ok(res);
    }

    private static async Task<Ok<string>> UpdateTodoItem(
        UpdateTodoItemCommand command,
        [AsParameters] TodoManagementServices services)
    {
        var res = await services.Mediator.Send(command);

        return TypedResults.Ok(res);
    }

    private static async Task<Ok<string>> RemoveTodoItem(
        [FromBody] RemoveTodoItemCommand command,
        [AsParameters] TodoManagementServices services)
    {
        var res = await services.Mediator.Send(command);

        return TypedResults.Ok(res);
    }

    private static async Task<Ok<string>> RegisterProgression(
        RegisterProgressionCommand command,
        [AsParameters] TodoManagementServices services)
    {
        var res = await services.Mediator.Send(command);

        return TypedResults.Ok(res);
    }

    private static async Task<Results<Ok, NotFound>> GenerateTodoListReport(
        [FromBody] GenerateTodoListReportCommand command,
        [AsParameters] TodoManagementServices services)
    {
        var success = await services.Mediator.Send(command);

        if (!success)
            return TypedResults.NotFound();

        return TypedResults.Ok();
    }

    #endregion

    #region Queries

    /// <summary>
    /// Gets a TodoList by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the TodoList.</param>
    /// <param name="services">The injected services.</param>
    /// <param name="includeItems">Whether to include the TodoItems collection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The TodoList ViewModel if found, otherwise NotFound.</returns>
    private static async Task<Results<Ok<TodoListViewModel>, NotFound>> GetTodoListById(
        Guid id,
        [AsParameters] TodoManagementServices services,
        bool includeItems = false,
        CancellationToken cancellationToken = default)
    {
        TodoList? todoList;
        if (includeItems)
        {
            // Use FirstOrDefaultAsync with includes when items are needed
            todoList = await services.TodoListQueries.FirstOrDefaultAsync(
                filter: tl => tl.Id == id,
                includes: query => query.Include(tl => tl.Items)
                                        .ThenInclude(i => i.Progressions.OrderByDescending(p => p.ActionDate)),
                cancellationToken);
        }
        else
        {
            // Use GetByIdAsync when items are not needed (more efficient)
            todoList = await services.TodoListQueries.GetByIdAsync(id, cancellationToken);
        }

        if (todoList == null)
            return TypedResults.NotFound();

        var viewModel = services.Mapper.ToViewModel(todoList);
        return TypedResults.Ok(viewModel);
    }

    /// <summary>
    /// Gets all TodoLists, optionally filtered and including Items.
    /// </summary>
    /// <param name="services">The injected services.</param>
    /// <param name="name">Optional filter by name (case-sensitive contains).</param>
    /// <param name="includeItems">Whether to include the TodoItems collection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of TodoList ViewModels.</returns>
    private static async Task<Ok<IReadOnlyList<TodoListViewModel>>> GetAllTodoLists(
        [AsParameters] TodoManagementServices services,
        string? name = null,
        bool includeItems = false,
        CancellationToken cancellationToken = default)
    {
        Expression<Func<TodoList, bool>>? filter = null;
        if (!string.IsNullOrWhiteSpace(name))
        {
            filter = tl => tl.Name.Contains(name);
        }

        var todoLists = await services.TodoListQueries.GetAllAsync(
            filter: filter,
            includes: includeItems ? query => query.Include(tl => tl.Items)
                                                   .ThenInclude(i => i.Progressions.OrderByDescending(p => p.ActionDate)) : null,
            cancellationToken);

        var viewModels = todoLists.Select(services.Mapper.ToViewModel).ToList();
        return TypedResults.Ok((IReadOnlyList<TodoListViewModel>)viewModels);
    }

    /// <summary>
    /// Gets all valid categories in the system.
    /// </summary>
    /// <param name="services">The injected services.</param>
    /// <returns>A list of valid category names.</returns>
    private static Ok<IReadOnlyList<string>> GetAllCategories()
    {
        var categories = CategoryMaster.ValidCategories.ToList();
        return TypedResults.Ok((IReadOnlyList<string>)categories);
    }

    #endregion
}