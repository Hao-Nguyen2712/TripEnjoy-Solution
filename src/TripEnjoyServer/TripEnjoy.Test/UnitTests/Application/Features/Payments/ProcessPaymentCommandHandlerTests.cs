using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using TripEnjoy.Application.Features.Payments.Commands;
using TripEnjoy.Application.Features.Payments.Handlers;
using TripEnjoy.Application.Interfaces.Logging;
using TripEnjoy.Application.Interfaces.Payment;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Booking;
using TripEnjoy.Domain.Booking.Entities;
using TripEnjoy.Domain.Booking.Enums;
using TripEnjoy.Domain.Booking.ValueObjects;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Property.ValueObjects;

namespace TripEnjoy.Test.UnitTests.Application.Features.Payments;

public class ProcessPaymentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly Mock<ILogger<ProcessPaymentCommandHandler>> _loggerMock;
    private readonly Mock<ILogService> _logServiceMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IGenericRepository<Booking>> _bookingRepositoryMock;
    private readonly Mock<IGenericRepository<Payment>> _paymentRepositoryMock;
    private readonly ProcessPaymentCommandHandler _handler;

    public ProcessPaymentCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _paymentServiceMock = new Mock<IPaymentService>();
        _loggerMock = new Mock<ILogger<ProcessPaymentCommandHandler>>();
        _logServiceMock = new Mock<ILogService>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _bookingRepositoryMock = new Mock<IGenericRepository<Booking>>();
        _paymentRepositoryMock = new Mock<IGenericRepository<Payment>>();

        _unitOfWorkMock.Setup(x => x.Repository<Booking>()).Returns(_bookingRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.Repository<Payment>()).Returns(_paymentRepositoryMock.Object);

        _handler = new ProcessPaymentCommandHandler(
            _unitOfWorkMock.Object,
            _paymentServiceMock.Object,
            _loggerMock.Object,
            _logServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnPaymentUrl()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var userId = UserId.CreateUnique();
        var propertyId = PropertyId.CreateUnique();
        var booking = Booking.Create(
            userId,
            propertyId,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(3),
            2,
            1000m).Value;

        var command = new ProcessPaymentCommand(
            bookingId,
            "VNPay",
            "https://localhost/payment/return");

        var paymentUrl = "https://payment-gateway.com/pay?orderId=123";

        _bookingRepositoryMock
            .Setup(x => x.GetByIdAsync(bookingId))
            .ReturnsAsync(booking);

        _paymentServiceMock
            .Setup(x => x.CreatePaymentUrlAsync(
                It.IsAny<string>(),
                It.IsAny<decimal>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(Result<string>.Success(paymentUrl));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(paymentUrl);

        _paymentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Payment>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentBooking_ShouldReturnFailure()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var command = new ProcessPaymentCommand(
            bookingId,
            "VNPay",
            "https://localhost/payment/return");

        _bookingRepositoryMock
            .Setup(x => x.GetByIdAsync(bookingId))
            .ReturnsAsync((Booking?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        _paymentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Payment>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonPendingBooking_ShouldReturnFailure()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var userId = UserId.CreateUnique();
        var propertyId = PropertyId.CreateUnique();
        var booking = Booking.Create(
            userId,
            propertyId,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(3),
            2,
            1000m).Value;
        booking.Confirm(); // Change status from Pending

        var command = new ProcessPaymentCommand(
            bookingId,
            "VNPay",
            "https://localhost/payment/return");

        _bookingRepositoryMock
            .Setup(x => x.GetByIdAsync(bookingId))
            .ReturnsAsync(booking);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Booking.InvalidStatus");
        _paymentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Payment>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidPaymentMethod_ShouldReturnFailure()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var userId = UserId.CreateUnique();
        var propertyId = PropertyId.CreateUnique();
        var booking = Booking.Create(
            userId,
            propertyId,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(3),
            2,
            1000m).Value;

        var command = new ProcessPaymentCommand(
            bookingId,
            "InvalidMethod",
            "https://localhost/payment/return");

        _bookingRepositoryMock
            .Setup(x => x.GetByIdAsync(bookingId))
            .ReturnsAsync(booking);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Payment.InvalidPaymentMethod");
    }

    [Fact]
    public async Task Handle_WhenPaymentServiceFails_ShouldMarkPaymentAsFailed()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var userId = UserId.CreateUnique();
        var propertyId = PropertyId.CreateUnique();
        var booking = Booking.Create(
            userId,
            propertyId,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(3),
            2,
            1000m).Value;

        var command = new ProcessPaymentCommand(
            bookingId,
            "VNPay",
            "https://localhost/payment/return");

        var error = new Error("Payment.ServiceError", "Payment service unavailable", ErrorType.Failure);

        _bookingRepositoryMock
            .Setup(x => x.GetByIdAsync(bookingId))
            .ReturnsAsync(booking);

        _paymentServiceMock
            .Setup(x => x.CreatePaymentUrlAsync(
                It.IsAny<string>(),
                It.IsAny<decimal>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(Result<string>.Failure(error));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        _paymentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Payment>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
