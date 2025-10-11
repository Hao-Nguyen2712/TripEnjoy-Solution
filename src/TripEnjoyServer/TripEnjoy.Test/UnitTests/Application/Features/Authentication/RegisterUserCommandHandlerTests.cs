using TripEnjoy.Application.Features.Authentication.Commands;
using TripEnjoy.Application.Features.Authentication.Handlers;
using TripEnjoy.Application.Interfaces.Identity;
using TripEnjoy.ShareKernel.Constant;

namespace TripEnjoy.Test.UnitTests.Application.Features.Authentication;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IAuthenService> _authenServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IGenericRepository<Account>> _accountRepositoryMock;
    private readonly RegisterUserCommandHandler _handler;
    private readonly IFixture _fixture;

    public RegisterUserCommandHandlerTests()
    {
        _authenServiceMock = new Mock<IAuthenService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _emailServiceMock = new Mock<IEmailService>();
        _accountRepositoryMock = new Mock<IGenericRepository<Account>>();
        _fixture = new Fixture();

        // Setup the unit of work to return our mocked repository
        _unitOfWorkMock.Setup(x => x.Repository<Account>())
            .Returns(_accountRepositoryMock.Object);

        _handler = new RegisterUserCommandHandler(
            _authenServiceMock.Object,
            _unitOfWorkMock.Object,
            _emailServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnAccountId()
    {
        // Arrange
        var email = "test@example.com";
        var password = "ValidPassword123!";
        var fullName = "John Doe";
        
        var command = new RegisterUserCommand(email, password, fullName);
        
        var aspNetUserId = _fixture.Create<string>();
        var confirmToken = _fixture.Create<string>();

        // Mock no existing account
        _accountRepositoryMock
            .Setup(x => x.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Account, bool>>>()))
            .ReturnsAsync((Account?)null);

        // Mock successful user creation
        _authenServiceMock
            .Setup(x => x.CreateUserAsync(email, password, RoleConstant.User))
            .ReturnsAsync(Result<(string UserId, string ConfirmToken)>.Success((aspNetUserId, confirmToken)));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        
        // Verify interactions
        _accountRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Account>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _authenServiceMock.Verify(x => x.CreateUserAsync(email, password, RoleConstant.User), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var email = "existing@example.com";
        var command = new RegisterUserCommand(email, "Password123!", "John Doe");
        
        var existingAccount = CreateValidAccount(email);

        _accountRepositoryMock
            .Setup(x => x.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Account, bool>>>()))
            .ReturnsAsync(existingAccount);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Account.DuplicateEmail);
        
        // Should not attempt to create user or save
        _authenServiceMock.Verify(x => x.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _accountRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Account>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserCreationFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterUserCommand("test@example.com", "Password123!", "John Doe");

        _accountRepositoryMock
            .Setup(x => x.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Account, bool>>>()))
            .ReturnsAsync((Account?)null);

        var userCreationError = new Error("Auth.UserCreationFailed", "Failed to create user", ErrorType.Failure);
        _authenServiceMock
            .Setup(x => x.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result<(string, string)>.Failure(userCreationError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(userCreationError);
        
        _accountRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Account>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithoutFullName_ShouldCreateAccountWithoutUserInfo()
    {
        // Arrange
        var command = new RegisterUserCommand("test@example.com", "Password123!", null);
        
        var aspNetUserId = _fixture.Create<string>();
        var confirmToken = _fixture.Create<string>();

        _accountRepositoryMock
            .Setup(x => x.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Account, bool>>>()))
            .ReturnsAsync((Account?)null);

        _authenServiceMock
            .Setup(x => x.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result<(string, string)>.Success((aspNetUserId, confirmToken)));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        _accountRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Account>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithEmptyFullName_ShouldCreateAccountWithoutUserInfo(string emptyFullName)
    {
        // Arrange
        var command = new RegisterUserCommand("test@example.com", "Password123!", emptyFullName);
        
        var aspNetUserId = _fixture.Create<string>();
        var confirmToken = _fixture.Create<string>();

        _accountRepositoryMock
            .Setup(x => x.GetAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Account, bool>>>()))
            .ReturnsAsync((Account?)null);

        _authenServiceMock
            .Setup(x => x.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result<(string, string)>.Success((aspNetUserId, confirmToken)));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        _accountRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Account>()), Times.Once);
    }

    private Account CreateValidAccount(string email)
    {
        var accountResult = Account.Create(_fixture.Create<string>(), email);
        return accountResult.Value;
    }
}