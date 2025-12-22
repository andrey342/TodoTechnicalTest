namespace TodoManagement.API.UnitTests.Application.Commands;

public class UpdateRemoveTests
{
    private readonly Mock<ITodoListRepository> _repositoryMock;
    private readonly TodoListMapper _mapper;
    
    public UpdateRemoveTests()
    {
        _repositoryMock = new Mock<ITodoListRepository>();
        _mapper = new TodoListMapper();
    }

    [Fact]
    public async Task UpdateHandler_Should_UpdateDescription_And_CallRepository()
    {
        // Arrange
        var handler = new UpdateTodoItemCommandHandler(_mapper, _repositoryMock.Object);
        var todoListId = Guid.NewGuid();
        var todoList = new TodoList("List");
        typeof(Entity).GetProperty("Id")!.SetValue(todoList, todoListId);
        todoList.AddItem(1, "Title", "Old Description", "Work");

        _repositoryMock.Setup(r => r.GetByIdAsync(
            todoListId, 
            It.IsAny<Func<IQueryable<TodoList>, IQueryable<TodoList>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(todoList);

        var command = new UpdateTodoItemCommand
        {
            TodoItem = new UpdateTodoItemDto
            {
                TodoListId = todoListId,
                ItemId = 1,
                Description = "New Description"
            }
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        todoList.Items.First(i => i.ItemId == 1).Description.Should().Be("New Description");
        _repositoryMock.Verify(r => r.UpdateAsync(todoList, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveHandler_Should_RemoveItem_And_CallRepository()
    {
        // Arrange
        var handler = new RemoveTodoItemCommandHandler(_mapper, _repositoryMock.Object);
        var todoListId = Guid.NewGuid();
        var todoList = new TodoList("List");
        typeof(Entity).GetProperty("Id")!.SetValue(todoList, todoListId);
        todoList.AddItem(1, "Title", "Desc", "Work");

        _repositoryMock.Setup(r => r.GetByIdAsync(
            todoListId, 
            It.IsAny<Func<IQueryable<TodoList>, IQueryable<TodoList>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(todoList);

        var command = new RemoveTodoItemCommand
        {
            TodoItem = new RemoveTodoItemDto
            {
                TodoListId = todoListId,
                ItemId = 1
            }
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        todoList.Items.Should().BeEmpty();
        _repositoryMock.Verify(r => r.UpdateAsync(todoList, It.IsAny<CancellationToken>()), Times.Once);
    }
}