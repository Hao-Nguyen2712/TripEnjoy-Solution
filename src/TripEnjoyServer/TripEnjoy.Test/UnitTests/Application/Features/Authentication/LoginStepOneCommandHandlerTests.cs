using TripEnjoy.Application.Features.Authentication.Commands;
using TripEnjoy.Application.Features.Authentication.Handlers;

namespace TripEnjoy.Test.UnitTests.Application.Features.Authentication;

public class LoginStepOneCommandHandlerTests
{
    private readonly Mock<IAuthenService> _authenServiceMock;
    private readonly LoginStepOneCommandHandler _handler;
    private readonly IFixture _fixture;

    public LoginStepOneCommandHandlerTests()
    {
        _authenServiceMock = new Mock<IAuthenService>();
        _fixture = new Fixture();

        _handler = new LoginStepOneCommandHandler(_authenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var email = "test@example.com";
        var password = "ValidPassword123!";
        var command = new LoginStepOneCommand(email, password);

        _authenServiceMock
            .Setup(x => x.LoginStepOneAsync(email, password))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        _authenServiceMock.Verify(x => x.LoginStepOneAsync(email, password), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidCredentials_ShouldReturnFailure()
    {
        // Arrange
        var email = "test@example.com";
        var password = "WrongPassword";
        var command = new LoginStepOneCommand(email, password);

        var authError = new Error("Auth.InvalidCredentials", "Invalid email or password", ErrorType.Validation);
        _authenServiceMock
            .Setup(x => x.LoginStepOneAsync(email, password))
            .ReturnsAsync(Result.Failure(authError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(authError);
        
        _authenServiceMock.Verify(x => x.LoginStepOneAsync(email, password), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var password = "SomePassword123!";
        var command = new LoginStepOneCommand(email, password);

        var notFoundError = new Error("Auth.UserNotFound", "User not found", ErrorType.NotFound);
        _authenServiceMock
            .Setup(x => x.LoginStepOneAsync(email, password))
            .ReturnsAsync(Result.Failure(notFoundError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(notFoundError);
        
        _authenServiceMock.Verify(x => x.LoginStepOneAsync(email, password), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInactiveAccount_ShouldReturnFailure()
    {
        // Arrange
        var email = "inactive@example.com";
        var password = "Password123!";
        var command = new LoginStepOneCommand(email, password);

        var inactiveError = new Error("Auth.AccountInactive", "Account is not active", ErrorType.Validation);
        _authenServiceMock
            .Setup(x => x.LoginStepOneAsync(email, password))
            .ReturnsAsync(Result.Failure(inactiveError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(inactiveError);
        
        _authenServiceMock.Verify(x => x.LoginStepOneAsync(email, password), Times.Once);
    }

    [Theory]
    [InlineData("", "ValidPassword123!")]
    [InlineData("test@example.com", "")]
    [InlineData("", "")]
    [InlineData("invalid-email", "ValidPassword123!")]
    public async Task Handle_WithInvalidInputs_ShouldCallServiceWithProvidedValues(string email, string password)
    {
        // Arrange
        var command = new LoginStepOneCommand(email, password);

        var validationError = new Error("Auth.ValidationFailed", "Validation failed", ErrorType.Validation);
        _authenServiceMock
            .Setup(x => x.LoginStepOneAsync(email, password))
            .ReturnsAsync(Result.Failure(validationError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        
        // Verify that the handler still calls the service with whatever values were provided
        _authenServiceMock.Verify(x => x.LoginStepOneAsync(email, password), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsException_ShouldPropagateException()
    {
        // Arrange
        var command = new LoginStepOneCommand("test@example.com", "Password123!");

        _authenServiceMock
            .Setup(x => x.LoginStepOneAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act & Assert
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Service error");
    }
}