using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TripEnjoy.Application.Features.Property.Commands;
using TripEnjoy.Application.Features.Property.Handlers;

namespace TripEnjoy.Test.UnitTests.Application.Features.Property;

public class CreatePropertyCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CreatePropertyCommandHandler>> _loggerMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IGenericRepository<Domain.Property.Property>> _propertyRepositoryMock;
    private readonly CreatePropertyCommandHandler _handler;
    private readonly IFixture _fixture;

    public CreatePropertyCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CreatePropertyCommandHandler>>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _propertyRepositoryMock = new Mock<IGenericRepository<Domain.Property.Property>>();
        _fixture = new Fixture();

        // Setup the unit of work to return our mocked repository
        _unitOfWorkMock.Setup(x => x.Repository<Domain.Property.Property>())
            .Returns(_propertyRepositoryMock.Object);

        _handler = new CreatePropertyCommandHandler(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _httpContextAccessorMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnPropertyId()
    {
        // Arrange
        var partnerIdGuid = Guid.NewGuid();
        var propertyTypeIdGuid = Guid.NewGuid();
        
        var command = new CreatePropertyCommand(
            PropertyTypeId: propertyTypeIdGuid,
            Name: "Beautiful Beach House",
            Address: "123 Ocean Drive",
            City: "Miami",
            Country: "USA",
            Description: "A stunning beachfront property",
            Latitude: 25.7617,
            Longitude: -80.1918);

        SetupHttpContextWithPartnerId(partnerIdGuid);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        
        _propertyRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Property.Property>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutPartnerIdClaim_ShouldReturnUnauthorizedFailure()
    {
        // Arrange
        var command = CreateValidCommand();
        
        SetupHttpContextWithoutPartnerId();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Type == ErrorType.Unauthorized);
        
        _propertyRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Property.Property>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidPartnerIdClaim_ShouldReturnUnauthorizedFailure()
    {
        // Arrange
        var command = CreateValidCommand();
        
        SetupHttpContextWithInvalidPartnerId("invalid-guid");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Type == ErrorType.Unauthorized);
        
        _propertyRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Property.Property>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyPropertyName_ShouldReturnValidationFailure()
    {
        // Arrange
        var partnerIdGuid = Guid.NewGuid();
        var command = new CreatePropertyCommand(
            PropertyTypeId: Guid.NewGuid(),
            Name: "", // Empty name should cause validation failure
            Address: "123 Test St",
            City: "Test City",
            Country: "Test Country",
            Description: "Test Description",
            Latitude: 40.7128,
            Longitude: -74.0060);

        SetupHttpContextWithPartnerId(partnerIdGuid);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        
        _propertyRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Property.Property>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithValidCommandAndOptionalFields_ShouldReturnPropertyId()
    {
        // Arrange
        var partnerIdGuid = Guid.NewGuid();
        var command = new CreatePropertyCommand(
            PropertyTypeId: Guid.NewGuid(),
            Name: "Test Property",
            Address: "123 Test St",
            City: "Test City",
            Country: "Test Country",
            Description: null, // Optional field
            Latitude: null,    // Optional field
            Longitude: null);  // Optional field

        SetupHttpContextWithPartnerId(partnerIdGuid);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        
        _propertyRepositoryMock.Verify(x => x.AddAsync(It.Is<Domain.Property.Property>(p => 
            p.Name == command.Name &&
            p.Address == command.Address &&
            p.Description == null &&
            p.Latitude == null &&
            p.Longitude == null)), Times.Once);
        
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var partnerIdGuid = Guid.NewGuid();
        var command = CreateValidCommand();
        
        SetupHttpContextWithPartnerId(partnerIdGuid);
        
        _propertyRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Domain.Property.Property>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ShouldPropagateException()
    {
        // Arrange
        var partnerIdGuid = Guid.NewGuid();
        var command = CreateValidCommand();
        
        SetupHttpContextWithPartnerId(partnerIdGuid);
        
        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Save failed"));

        // Act & Assert
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Save failed");
    }

    [Theory]
    [InlineData(-90.1, -180.0)] // Invalid latitude (too low)
    [InlineData(90.1, 180.0)]   // Invalid latitude (too high)
    [InlineData(0.0, -180.1)]   // Invalid longitude (too low)
    [InlineData(0.0, 180.1)]    // Invalid longitude (too high)
    public async Task Handle_WithInvalidCoordinates_ShouldStillCreateProperty(double? latitude, double? longitude)
    {
        // Note: Based on the current domain implementation, the Property.Create method
        // doesn't validate coordinate bounds, so this test verifies current behavior
        // Arrange
        var partnerIdGuid = Guid.NewGuid();
        var command = new CreatePropertyCommand(
            PropertyTypeId: Guid.NewGuid(),
            Name: "Test Property",
            Address: "123 Test St",
            City: "Test City",
            Country: "Test Country",
            Description: "Test property with invalid coordinates",
            Latitude: latitude,
            Longitude: longitude);

        SetupHttpContextWithPartnerId(partnerIdGuid);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Current implementation allows invalid coordinates
        result.IsSuccess.Should().BeTrue();
        
        _propertyRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Property.Property>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #region Helper Methods

    private CreatePropertyCommand CreateValidCommand()
    {
        return new CreatePropertyCommand(
            PropertyTypeId: Guid.NewGuid(),
            Name: "Test Property",
            Address: "123 Test Street",
            City: "Test City",
            Country: "Test Country",
            Description: "A test property",
            Latitude: 40.7128,
            Longitude: -74.0060);
    }

    private void SetupHttpContextWithPartnerId(Guid partnerIdGuid)
    {
        var claims = new List<Claim>
        {
            new("PartnerId", partnerIdGuid.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        
        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext)
            .Returns(httpContext);
    }

    private void SetupHttpContextWithoutPartnerId()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity())
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext)
            .Returns(httpContext);
    }

    private void SetupHttpContextWithInvalidPartnerId(string invalidPartnerId)
    {
        var claims = new List<Claim>
        {
            new("PartnerId", invalidPartnerId)
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        
        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext)
            .Returns(httpContext);
    }

    #endregion
}