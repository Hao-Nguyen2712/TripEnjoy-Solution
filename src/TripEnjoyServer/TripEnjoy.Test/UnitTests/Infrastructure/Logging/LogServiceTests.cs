using Microsoft.Extensions.Logging;
using Moq;
using TripEnjoy.Infrastructure.Logging;
using Xunit;

namespace TripEnjoy.Test.UnitTests.Infrastructure.Logging;

public class LogServiceTests
{
    private readonly Mock<ILogger<LogService>> _mockLogger;
    private readonly LogService _logService;

    public LogServiceTests()
    {
        _mockLogger = new Mock<ILogger<LogService>>();
        _logService = new LogService(_mockLogger.Object);
    }

    [Fact]
    public void LogInfo_ShouldCallLogInformation()
    {
        // Arrange
        var message = "Test info message";
        var args = new object[] { "arg1", "arg2" };

        // Act
        _logService.LogInfo(message, args);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void LogWarning_ShouldCallLogWarning()
    {
        // Arrange
        var message = "Test warning message";
        var args = new object[] { "warning1" };

        // Act
        _logService.LogWarning(message, args);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void LogError_ShouldCallLogErrorWithException()
    {
        // Arrange
        var message = "Test error message";
        var exception = new Exception("Test exception");
        var args = new object[] { "error1" };

        // Act
        _logService.LogError(exception, message, args);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                exception,
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void LogDebug_ShouldCallLogDebug()
    {
        // Arrange
        var message = "Test debug message";
        var args = new object[] { "debug1" };

        // Act
        _logService.LogDebug(message, args);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void LogPerformance_WithoutAdditionalData_ShouldLogInformationWithMetrics()
    {
        // Arrange
        var operationName = "TestOperation";
        var elapsedMilliseconds = 1500L;

        // Act
        _logService.LogPerformance(operationName, elapsedMilliseconds);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void LogPerformance_WithAdditionalData_ShouldLogInformationWithAllData()
    {
        // Arrange
        var operationName = "TestOperation";
        var elapsedMilliseconds = 2500L;
        var additionalData = new Dictionary<string, object>
        {
            ["RequestId"] = "123",
            ["UserId"] = "user-456"
        };

        // Act
        _logService.LogPerformance(operationName, elapsedMilliseconds, additionalData);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void LogSecurity_WithoutAdditionalData_ShouldLogWarning()
    {
        // Arrange
        var eventType = "UnauthorizedAccess";
        var message = "User attempted to access restricted resource";

        // Act
        _logService.LogSecurity(eventType, message);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void LogSecurity_WithAdditionalData_ShouldLogWarningWithAllData()
    {
        // Arrange
        var eventType = "FailedLogin";
        var message = "Invalid credentials";
        var additionalData = new Dictionary<string, object>
        {
            ["IpAddress"] = "192.168.1.1",
            ["AttemptCount"] = 3
        };

        // Act
        _logService.LogSecurity(eventType, message, additionalData);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void LogBusinessEvent_WithoutEventData_ShouldLogInformation()
    {
        // Arrange
        var eventName = "BookingCreated";

        // Act
        _logService.LogBusinessEvent(eventName);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void LogBusinessEvent_WithEventData_ShouldLogInformationWithAllData()
    {
        // Arrange
        var eventName = "PropertyCreated";
        var eventData = new Dictionary<string, object>
        {
            ["PropertyId"] = "prop-123",
            ["PartnerId"] = "partner-456",
            ["PropertyName"] = "Test Hotel"
        };

        // Act
        _logService.LogBusinessEvent(eventName, eventData);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }
}
