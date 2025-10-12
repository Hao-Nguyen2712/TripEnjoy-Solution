using TripEnjoy.Application.Features.Authentication.Commands;
using TripEnjoy.Application.Features.Authentication.Handlers;
using TripEnjoy.Application.Interfaces.Identity;
using TripEnjoy.ShareKernel.Constant;
using TripEnjoy.Domain.Common.Errors;

namespace TripEnjoy.Test.UnitTests.Application.Features.Authentication;

public class LoginPartnerStepOneCommandHandlerTests
{
    private readonly Mock<IAuthenService> _authenServiceMock;
    private readonly LoginPartnerStepOneCommandHandler _handler;
    private readonly IFixture _fixture;

    public LoginPartnerStepOneCommandHandlerTests()
    {
        _authenServiceMock = new Mock<IAuthenService>();
        _fixture = new Fixture();
        _handler = new LoginPartnerStepOneCommandHandler(_authenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidPartnerCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var email = "partner@example.com";
        var password = "ValidPassword123!";
        
        var command = new LoginPartnerStepOneCommand(email, password);
        
        // Mock successful authentication with Partner role validation
        _authenServiceMock
            .Setup(x => x.LoginStepOneAsync(email, password, RoleConstant.Partner))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // Verify the service was called with the correct role
        _authenServiceMock.Verify(x => x.LoginStepOneAsync(email, password, RoleConstant.Partner), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidCredentials_ShouldReturnFailure()
    {
        // Arrange
        var email = "invalid@example.com";
        var password = "WrongPassword";
        
        var command = new LoginPartnerStepOneCommand(email, password);
        
        var loginFailedError = DomainError.Account.LoginFailed;
        _authenServiceMock
            .Setup(x => x.LoginStepOneAsync(email, password, RoleConstant.Partner))
            .ReturnsAsync(Result.Failure(loginFailedError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(loginFailedError);
        
        _authenServiceMock.Verify(x => x.LoginStepOneAsync(email, password, RoleConstant.Partner), Times.Once);
    }

    [Fact]
    public async Task Handle_WithRoleMismatch_ShouldReturnRoleMismatchError()
    {
        // Arrange - Account with User role trying to use Partner endpoint
        var email = "user@example.com";
        var password = "ValidPassword123!";
        
        var command = new LoginPartnerStepOneCommand(email, password);
        
        var roleMismatchError = DomainError.Account.RoleMismatch;
        _authenServiceMock
            .Setup(x => x.LoginStepOneAsync(email, password, RoleConstant.Partner))
            .ReturnsAsync(Result.Failure(roleMismatchError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(roleMismatchError);
        
        _authenServiceMock.Verify(x => x.LoginStepOneAsync(email, password, RoleConstant.Partner), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Handle_WithInvalidEmail_ShouldStillCallService(string invalidEmail)
    {
        // Arrange - Handler should pass validation to service and let FluentValidation handle it
        var password = "ValidPassword123!";
        
        var command = new LoginPartnerStepOneCommand(invalidEmail, password);
        
        _authenServiceMock
            .Setup(x => x.LoginStepOneAsync(invalidEmail, password, RoleConstant.Partner))
            .ReturnsAsync(Result.Failure(DomainError.Account.LoginFailed));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        
        _authenServiceMock.Verify(x => x.LoginStepOneAsync(invalidEmail, password, RoleConstant.Partner), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldAlwaysPassPartnerRoleConstant()
    {
        // Arrange
        var command = new LoginPartnerStepOneCommand("test@example.com", "password");
        
        _authenServiceMock
            .Setup(x => x.LoginStepOneAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Success());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - Verify it always uses Partner role constant, never User or other roles
        _authenServiceMock.Verify(x => x.LoginStepOneAsync(
            command.Email, 
            command.Password, 
            RoleConstant.Partner), Times.Once);
        
        _authenServiceMock.Verify(x => x.LoginStepOneAsync(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            RoleConstant.User), Times.Never);
    }

    [Fact]
    public async Task Handle_WithLockedAccount_ShouldReturnLockedOutError()
    {
        // Arrange
        var email = "locked@example.com";
        var password = "ValidPassword123!";
        
        var command = new LoginPartnerStepOneCommand(email, password);
        
        var lockedOutError = DomainError.Account.LockedOut;
        _authenServiceMock
            .Setup(x => x.LoginStepOneAsync(email, password, RoleConstant.Partner))
            .ReturnsAsync(Result.Failure(lockedOutError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(lockedOutError);
        
        _authenServiceMock.Verify(x => x.LoginStepOneAsync(email, password, RoleConstant.Partner), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUnconfirmedEmail_ShouldReturnEmailNotConfirmedError()
    {
        // Arrange
        var email = "unconfirmed@example.com";
        var password = "ValidPassword123!";
        
        var command = new LoginPartnerStepOneCommand(email, password);
        
        var emailNotConfirmedError = DomainError.Account.EmailNotConfirmed;
        _authenServiceMock
            .Setup(x => x.LoginStepOneAsync(email, password, RoleConstant.Partner))
            .ReturnsAsync(Result.Failure(emailNotConfirmedError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(emailNotConfirmedError);
        
        _authenServiceMock.Verify(x => x.LoginStepOneAsync(email, password, RoleConstant.Partner), Times.Once);
    }
}