namespace TodoManagement.API.UnitTests.Application.Commands.Create;

public class CreateTodoListTests
{
    private readonly Mock<ITodoListRepository> _repositoryMock;
    private readonly TodoListMapper _mapper;
    private readonly CreateTodoListCommandHandler _handler;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IValidationOnlyRepository<TodoList>> _validationRepoMock;
    private readonly CreateTodoListCommandValidator _validator;

    public CreateTodoListTests()
    {
        // Handler Setup
        _repositoryMock = new Mock<ITodoListRepository>();
        _mapper = new TodoListMapper();
        _handler = new CreateTodoListCommandHandler(_mapper, _repositoryMock.Object);

        // Validator Setup
        _serviceProviderMock = new Mock<IServiceProvider>();
        _validationRepoMock = new Mock<IValidationOnlyRepository<TodoList>>();
        
        // Mock IServiceProvider to return the validation repo
        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IValidationOnlyRepository<TodoList>)))
            .Returns(_validationRepoMock.Object);

        _validator = new CreateTodoListCommandValidator(_serviceProviderMock.Object);
    }

    [Fact]
    public async Task Handle_Should_CallRepositoryAndReturnId()
    {
        // Arrange
        var command = new CreateTodoListCommand
        {
            TodoList = new TodoListDto { Name = "New List" }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNullOrEmpty();
        _repositoryMock.Verify(r => r.AddAsync(It.Is<TodoList>(t => t.Name == "New List"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Validate_Should_Succeed_When_NameIsValidAndUnique()
    {
        // Arrange
        var command = new CreateTodoListCommand
        {
            TodoList = new TodoListDto { Name = "Unique Name" }
        };

        // Mock uniqueness check to return false (meaning no duplicate exists)
        _validationRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<TodoList, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_Should_Fail_When_NameIsNull()
    {
        // Arrange
        var command = new CreateTodoListCommand
        {
            TodoList = new TodoListDto { Name = "" } // Invalid
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TodoList.Name);
    }

    [Fact]
    public async Task Validate_Should_Fail_When_NameIsDuplicate()
    {
        // Arrange
        var command = new CreateTodoListCommand
        {
            TodoList = new TodoListDto { Name = "Duplicate Name" }
        };

        // Mock uniqueness check to return true (meaning duplicate exists)
        _validationRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<TodoList, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TodoList.Name)
              .WithErrorMessage("Name already exists.");
    }
}