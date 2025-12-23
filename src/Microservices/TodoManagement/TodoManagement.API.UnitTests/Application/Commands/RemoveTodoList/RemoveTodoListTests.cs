namespace TodoManagement.API.UnitTests.Application.Commands.RemoveTodoList;

public class RemoveTodoListTests
{
    private readonly Mock<ITodoListRepository> _repositoryMock;
    
    public RemoveTodoListTests()
    {
        _repositoryMock = new Mock<ITodoListRepository>();
    }

    [Fact]
    public async Task Handle_Should_DeleteTodoList_When_AllItemsAreModifiable()
    {
        // Arrange
        var handler = new RemoveTodoListCommandHandler(_repositoryMock.Object);
        var todoListId = Guid.NewGuid();
        var todoList = new TodoList("List");
        typeof(Entity).GetProperty("Id")!.SetValue(todoList, todoListId);
        
        // Add items with < 50% progress (default is 0)
        todoList.AddItem(1, "Title1", "Desc1", "Work");
        todoList.AddItem(2, "Title2", "Desc2", "Work");

        _repositoryMock.Setup(r => r.GetByIdAsync(
            todoListId, 
            It.IsAny<Func<IQueryable<TodoList>, IQueryable<TodoList>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(todoList);

        var command = new RemoveTodoListCommand(todoListId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync(todoList, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowException_When_ContainsItemWithHighProgress()
    {
        // Arrange
        var handler = new RemoveTodoListCommandHandler(_repositoryMock.Object);
        var todoListId = Guid.NewGuid();
        var todoList = new TodoList("List");
        typeof(Entity).GetProperty("Id")!.SetValue(todoList, todoListId);
        
        // Item 1: 0%
        todoList.AddItem(1, "Title1", "Desc1", "Work");
        
        // Item 2: 60%
        todoList.AddItem(2, "Title2", "Desc2", "Work");
        
        // Add progression to item 2 to make it > 50%
        // We use the domain method
        todoList.RegisterProgression(2, DateTime.UtcNow, 60m);

        _repositoryMock.Setup(r => r.GetByIdAsync(
            todoListId, 
            It.IsAny<Func<IQueryable<TodoList>, IQueryable<TodoList>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(todoList);

        var command = new RemoveTodoListCommand(todoListId);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            handler.Handle(command, CancellationToken.None).AsTask());
            
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<TodoList>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_Should_DeleteTodoList_When_ListIsEmpty()
    {
        // Arrange
        var handler = new RemoveTodoListCommandHandler(_repositoryMock.Object);
        var todoListId = Guid.NewGuid();
        var todoList = new TodoList("List");
        typeof(Entity).GetProperty("Id")!.SetValue(todoList, todoListId);
        
        _repositoryMock.Setup(r => r.GetByIdAsync(
            todoListId, 
            It.IsAny<Func<IQueryable<TodoList>, IQueryable<TodoList>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(todoList);

        var command = new RemoveTodoListCommand(todoListId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync(todoList, It.IsAny<CancellationToken>()), Times.Once);
    }
}