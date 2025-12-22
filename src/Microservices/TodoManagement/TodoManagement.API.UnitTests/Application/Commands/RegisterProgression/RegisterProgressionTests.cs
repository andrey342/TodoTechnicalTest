namespace TodoManagement.API.UnitTests.Application.Commands.RegisterProgression;

public class RegisterProgressionTests
{
    private readonly Mock<ITodoListRepository> _repositoryMock;
    private readonly TodoListMapper _mapper;
    private readonly RegisterProgressionCommandHandler _handler;

    public RegisterProgressionTests()
    {
        _repositoryMock = new Mock<ITodoListRepository>();
        _mapper = new TodoListMapper();
        _handler = new RegisterProgressionCommandHandler(_mapper, _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_RegisterProgression_And_UpdateRepository()
    {
        // Arrange
        var todoListId = Guid.NewGuid();
        var todoList = new TodoList("Test List");
        // Reflection to set Id
        typeof(Entity).GetProperty("Id")!.SetValue(todoList, todoListId);
        
        // Add an item to the list so we can add progression to it
        todoList.AddItem(1, "Title", "Desc", "Work");

        _repositoryMock.Setup(r => r.GetByIdAsync(
                todoListId, 
                It.IsAny<Func<IQueryable<TodoList>, IQueryable<TodoList>>>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(todoList);

        var command = new RegisterProgressionCommand
        {
            TodoListId = todoListId,
            ItemId = 1,
            Progression = new RegisterProgressionDto
            {
                ActionDate = DateTime.UtcNow,
                Percent = 10m
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        
        // Verify progression was added (Domain Logic check via public state)
        var item = todoList.Items.First(i => i.ItemId == 1);
        item.GetTotalProgress().Should().Be(10m);

        // Verify UpdateAsync
        _repositoryMock.Verify(r => r.UpdateAsync(todoList, It.IsAny<CancellationToken>()), Times.Once);
    }
}