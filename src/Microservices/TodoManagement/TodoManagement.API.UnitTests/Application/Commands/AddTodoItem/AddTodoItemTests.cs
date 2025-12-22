namespace TodoManagement.API.UnitTests.Application.Commands.AddTodoItem;

public class AddTodoItemTests
{
    private readonly Mock<ITodoListRepository> _repositoryMock;
    private readonly TodoListMapper _mapper;
    private readonly AddTodoItemCommandHandler _handler;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IValidationOnlyRepository<TodoList>> _validationRepoMock;
    private readonly AddTodoItemCommandValidator _validator;

    public AddTodoItemTests()
    {
        // Handler Setup
        _repositoryMock = new Mock<ITodoListRepository>();
        _mapper = new TodoListMapper();
        _handler = new AddTodoItemCommandHandler(_mapper, _repositoryMock.Object);

        // Validator Setup
        _serviceProviderMock = new Mock<IServiceProvider>();
        _validationRepoMock = new Mock<IValidationOnlyRepository<TodoList>>();

        // Mock IServiceProvider logic for BaseValidator
        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IValidationOnlyRepository<TodoList>)))
            .Returns(_validationRepoMock.Object);

        _validator = new AddTodoItemCommandValidator(_serviceProviderMock.Object);
    }

    [Fact]
    public async Task Handle_Should_AddItemAndCallUpdate()
    {
        // Arrange
        var todoListId = Guid.NewGuid();
        var todoList = new TodoList("Test List");
        // Reflection to set Id since setter is private/init
        typeof(Entity).GetProperty("Id")!.SetValue(todoList, todoListId);

        _repositoryMock.Setup(r => r.GetByIdAsync(todoListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(todoList);
        _repositoryMock.Setup(r => r.GetNextId()).Returns(101);

        var command = new AddTodoItemCommand
        {
            TodoItem = new AddTodoItemDto
            {
                TodoListId = todoListId,
                Title = "Item 101",
                Description = "Description",
                Category = "Work"
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        todoList.Items.Should().Contain(i => i.ItemId == 101 && i.Title == "Item 101");
        
        // Verify UpdateAsync is called
        _repositoryMock.Verify(r => r.UpdateAsync(todoList, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Validate_Should_Succeed_When_TodoListExistsAndFieldsValid()
    {
        // Arrange
        var todoListId = Guid.NewGuid();
        var command = new AddTodoItemCommand
        {
            TodoItem = new AddTodoItemDto
            {
                TodoListId = todoListId,
                Title = "Valid Title",
                Description = "Valid Description",
                Category = "Valid Category"
            }
        };

        // Mock existence check
        _validationRepoMock
            .Setup(r => r.ExistsAsync(todoListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_Should_Fail_When_TodoListDoesNotExist()
    {
        // Arrange
        var todoListId = Guid.NewGuid();
        var command = new AddTodoItemCommand
        {
            TodoItem = new AddTodoItemDto
            {
                TodoListId = todoListId,
                Title = "Title",
                Description = "Desc",
                Category = "Cat"
            }
        };

        // Mock existence check to return false
        _validationRepoMock
            .Setup(r => r.ExistsAsync(todoListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TodoItem.TodoListId);
    }
}