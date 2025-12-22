namespace TodoManagement.API.UnitTests.Application.Common.Idempotency;

public class IdempotentCommandValidatorTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly IdempotentCommandValidator<TestIdempotentCommand> _validator;

    public IdempotentCommandValidatorTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _validator = new IdempotentCommandValidator<TestIdempotentCommand>(_serviceProviderMock.Object);
    }

    [Fact]
    public void Validate_Should_Succeed_When_RequestIdIsValid()
    {
        // Arrange
        var command = new TestIdempotentCommand(Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Should_Fail_When_RequestIdIsEmpty()
    {
        // Arrange
        var command = new TestIdempotentCommand(Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RequestId)
              .WithErrorMessage("RequestId is required.");
    }

    // Dummy command for testing
    public record TestIdempotentCommand(Guid RequestId) : IIdempotentRequest;
}