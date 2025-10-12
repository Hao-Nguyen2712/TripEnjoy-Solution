using TripEnjoy.Application.Features.Authentication.Commands;
using TripEnjoy.Application.Features.Authentication.Handlers;
using TripEnjoy.Application.Interfaces.Identity;
using TripEnjoy.ShareKernel.Constant;
using TripEnjoy.Domain.Common.Errors;

namespace TripEnjoy.Test.UnitTests.Application.Features.Authentication;

public class LoginUserStepOneCommandHandlerTests
{
    private readonly Mock<IAuthenService> _authenServiceMock;
    private readonly LoginUserStepOneCommandHandler _handler;
    private readonly IFixture _fixture;

    public LoginUserStepOneCommandHandlerTests()
    {
        _authenServiceMock = new Mock<IAuthenService>();
        _fixture = new Fixture();
        _handler = new LoginUserStepOneCommandHandler(_authenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidUserCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var email = "user@example.com";
        var password = "ValidPassword123!";
        
        var command = new LoginUserStepOneCommand(email, password);
        
        // Mock successful authentication with User role validation
        _authenServiceMock
            .Setup(x => x.LoginStepOneAsync(email, password, RoleConstant.User))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // Verify the service was called with the correct role
        _authenServiceMock.Verify(x => x.LoginStepOneAsync(email, password, RoleConstant.User), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidCredentials_ShouldReturnFailure()
    {
        // Arrange
        var email = "invalid@example.com";
        var password = "WrongPassword";
        
        var command = new LoginUserStepOneCommand(email, password);
        
        var loginFailedError = DomainError.Account.LoginFailed;
        _authenServiceMock
            .Setup(x => x.LoginStepOneAsync(email, password, RoleConstant.User))
            .ReturnsAsync(Result.Failure(loginFailedError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(loginFailedError);
        
        _authenServiceMock.Verify(x => x.LoginStepOneAsync(email, password, RoleConstant.User), Times.Once);
    }

    [Fact]
    public async Task Handle_WithRoleMismatch_ShouldReturnRoleMismatchError()
    {
        // Arrange - Account with Partner role trying to use User endpoint
        var email = "partner@example.com";
        var password = "ValidPassword123!";
        
        var command = new LoginUserStepOneCommand(email, password);
        
        var roleMismatchError = DomainError.Account.RoleMismatch;
        _authenServiceMock
            .Setup(x => x.LoginStepOneAsync(email, password, RoleConstant.User))
            .ReturnsAsync(Result.Failure(roleMismatchError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(roleMismatchError);
        
        _authenServiceMock.Verify(x => x.LoginStepOneAsync(email, password, RoleConstant.User), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Handle_WithInvalidEmail_ShouldStillCallService(string invalidEmail)
    {
        // Arrange - Handler should pass validation to service and let FluentValidation handle it
        var password = "ValidPassword123!";
        
        var command = new LoginUserStepOneCommand(invalidEmail, password);
        
        _authenServiceMock
            .Setup(x => x.LoginStepOneAsync(invalidEmail, password, RoleConstant.User))
            .ReturnsAsync(Result.Failure(DomainError.Account.LoginFailed));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        
        _authenServiceMock.Verify(x => x.LoginStepOneAsync(invalidEmail, password, RoleConstant.User), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldAlwaysPassUserRoleConstant()
    {
        // Arrange
        var command = new LoginUserStepOneCommand("test@example.com", "password");
        
        _authenServiceMock
            .Setup(x => x.LoginStepOneAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Success());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - Verify it always uses User role constant, never Partner or other roles
        _authenServiceMock.Verify(x => x.LoginStepOneAsync(
            command.Email, 
            command.Password, 
            RoleConstant.User), Times.Once);
        
        _authenServiceMock.Verify(x => x.LoginStepOneAsync(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            RoleConstant.Partner), Times.Never);
    }
}