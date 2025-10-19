using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using TripEnjoy.Application.Features.Authentication.Commands;
using TripEnjoy.Application.Features.Authentication.Handlers;
using TripEnjoy.Application.Interfaces.Identity;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account;
using TripEnjoy.Domain.Account.Entities;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Test.UnitTests.Application.Features.Authentication
{
    public class RefreshTokenCommandHandlerTests
    {
        private readonly Mock<IAuthenService> _mockAuthenService;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IAccountRepository> _mockAccountRepository;
        private readonly Mock<ILogger<RefreshTokenCommandHandler>> _mockLogger;
        private readonly RefreshTokenCommandHandler _handler;

        public RefreshTokenCommandHandlerTests()
        {
            _mockAuthenService = new Mock<IAuthenService>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockAccountRepository = new Mock<IAccountRepository>();
            _mockLogger = new Mock<ILogger<RefreshTokenCommandHandler>>();

            _mockUnitOfWork.Setup(x => x.AccountRepository).Returns(_mockAccountRepository.Object);

            _handler = new RefreshTokenCommandHandler(
                _mockAuthenService.Object,
                _mockUnitOfWork.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_WithValidRefreshToken_ShouldReturnNewTokens()
        {
            // Arrange
            var aspNetUserId = Guid.NewGuid().ToString();
            var oldRefreshToken = "old_refresh_token";
            var newRefreshToken = "new_refresh_token";
            var newAccessToken = "new_access_token";
            var expiredAccessToken = "expired_access_token";

            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, aspNetUserId)
            }));

            var account = Account.Create(aspNetUserId, "test@example.com").Value;
            // Add the old refresh token to the account
            account.AddRefreshToken(oldRefreshToken);

            var command = new RefreshTokenCommand(expiredAccessToken, oldRefreshToken);

            _mockAuthenService.Setup(x => x.GetPrincipalFromExpiredToken(oldRefreshToken))
                .ReturnsAsync(Result<ClaimsPrincipal?>.Success(principal));

            _mockAccountRepository.Setup(x => x.FindByAspNetUserIdWithBlackListTokensAsync(aspNetUserId))
                .ReturnsAsync(account);

            _mockAuthenService.Setup(x => x.GenerateRefreshToken())
                .Returns(newRefreshToken);

            _mockAuthenService.Setup(x => x.GenerateAccessTokenAsync(aspNetUserId))
                .ReturnsAsync(newAccessToken);

            _mockAccountRepository.Setup(x => x.UpdateAsync(It.IsAny<Account>()))
                .ReturnsAsync((Account acc) => acc);

            _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Token.Should().Be(newAccessToken);
            result.Value.RefreshToken.Should().Be(newRefreshToken);
            result.Value.AspNetUserId.Should().Be(aspNetUserId);

            // Verify the old token was revoked
            var oldToken = account.RefreshTokens.FirstOrDefault(rt => rt.Token == oldRefreshToken);
            oldToken.Should().NotBeNull();
            oldToken!.IsUsed.Should().BeTrue();

            // Verify the new token was added
            var newToken = account.RefreshTokens.FirstOrDefault(rt => rt.Token == newRefreshToken);
            newToken.Should().NotBeNull();
            newToken!.IsUsed.Should().BeFalse();

            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithInvalidRefreshToken_ShouldReturnFailure()
        {
            // Arrange
            var command = new RefreshTokenCommand("expired_access_token", "invalid_refresh_token");

            _mockAuthenService.Setup(x => x.GetPrincipalFromExpiredToken("invalid_refresh_token"))
                                .ReturnsAsync(Result<ClaimsPrincipal?>.Failure(DomainError.RefreshToken.InvalidToken));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(DomainError.Account.InvalidToken);
        }

        [Fact]
        public async Task Handle_WithNullPrincipal_ShouldReturnFailure()
        {
            // Arrange
            var command = new RefreshTokenCommand("expired_access_token", "refresh_token");

            _mockAuthenService.Setup(x => x.GetPrincipalFromExpiredToken("refresh_token"))
                .ReturnsAsync(Result<ClaimsPrincipal?>.Success(null));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(DomainError.Account.InvalidToken);
        }

        [Fact]
        public async Task Handle_WithMissingUserIdClaim_ShouldReturnFailure()
        {
            // Arrange
            var principal = new ClaimsPrincipal(new ClaimsIdentity()); // No NameIdentifier claim
            var command = new RefreshTokenCommand("expired_access_token", "refresh_token");

            _mockAuthenService.Setup(x => x.GetPrincipalFromExpiredToken("refresh_token"))
                .ReturnsAsync(Result<ClaimsPrincipal?>.Success(principal));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(DomainError.Account.InvalidToken);
        }

        [Fact]
        public async Task Handle_WithNonExistentAccount_ShouldReturnFailure()
        {
            // Arrange
            var aspNetUserId = Guid.NewGuid().ToString();
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, aspNetUserId)
            }));

            var command = new RefreshTokenCommand("expired_access_token", "refresh_token");

            _mockAuthenService.Setup(x => x.GetPrincipalFromExpiredToken("refresh_token"))
                .ReturnsAsync(Result<ClaimsPrincipal?>.Success(principal));

            _mockAccountRepository.Setup(x => x.FindByAspNetUserIdWithBlackListTokensAsync(aspNetUserId))
                .ReturnsAsync((Account)null!);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(DomainError.Account.NotFound);
        }

        [Fact]
        public async Task Handle_WithBlacklistedToken_ShouldReturnFailure()
        {
            // Arrange
            var aspNetUserId = Guid.NewGuid().ToString();
            var blacklistedToken = "blacklisted_token";

            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, aspNetUserId)
            }));

            var account = Account.Create(aspNetUserId, "test@example.com").Value;
            account.AddBlackListToken(blacklistedToken, DateTime.UtcNow.AddDays(7));

            var command = new RefreshTokenCommand("expired_access_token", blacklistedToken);

            _mockAuthenService.Setup(x => x.GetPrincipalFromExpiredToken(blacklistedToken))
                .ReturnsAsync(Result<ClaimsPrincipal?>.Success(principal));

            _mockAccountRepository.Setup(x => x.FindByAspNetUserIdWithBlackListTokensAsync(aspNetUserId))
                .ReturnsAsync(account);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(DomainError.Account.InvalidToken);
        }

        [Fact]
        public async Task Handle_WithTokenNotFoundInAccount_ShouldReturnFailure()
        {
            // Arrange
            var aspNetUserId = Guid.NewGuid().ToString();
            var refreshToken = "non_existent_token";

            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, aspNetUserId)
            }));

            var account = Account.Create(aspNetUserId, "test@example.com").Value;
            // Don't add the refresh token to the account

            var command = new RefreshTokenCommand("expired_access_token", refreshToken);

            _mockAuthenService.Setup(x => x.GetPrincipalFromExpiredToken(refreshToken))
                .ReturnsAsync(Result<ClaimsPrincipal?>.Success(principal));

            _mockAccountRepository.Setup(x => x.FindByAspNetUserIdWithBlackListTokensAsync(aspNetUserId))
                .ReturnsAsync(account);

            _mockAuthenService.Setup(x => x.GenerateRefreshToken())
                .Returns("new_token");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(DomainError.RefreshToken.RefreshTokenNotFound);
        }

        [Fact]
        public async Task Handle_WithAlreadyUsedToken_ShouldReturnFailure()
        {
            // Arrange
            var aspNetUserId = Guid.NewGuid().ToString();
            var usedToken = "used_token";

            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, aspNetUserId)
            }));

            var account = Account.Create(aspNetUserId, "test@example.com").Value;
            account.AddRefreshToken(usedToken);
            
            // Revoke the token to mark it as used
            account.RevokeRefreshToken(usedToken);

            var command = new RefreshTokenCommand("expired_access_token", usedToken);

            _mockAuthenService.Setup(x => x.GetPrincipalFromExpiredToken(usedToken))
                .ReturnsAsync(Result<ClaimsPrincipal?>.Success(principal));

            _mockAccountRepository.Setup(x => x.FindByAspNetUserIdWithBlackListTokensAsync(aspNetUserId))
                .ReturnsAsync(account);

            _mockAuthenService.Setup(x => x.GenerateRefreshToken())
                .Returns("new_token");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(DomainError.RefreshToken.RefreshTokenInvalidated);
        }

        [Fact]
        public async Task Handle_WhenSaveChangesFails_ShouldReturnFailure()
        {
            // Arrange
            var aspNetUserId = Guid.NewGuid().ToString();
            var oldRefreshToken = "old_refresh_token";
            var newRefreshToken = "new_refresh_token";
            var newAccessToken = "new_access_token";

            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, aspNetUserId)
            }));

            var account = Account.Create(aspNetUserId, "test@example.com").Value;
            account.AddRefreshToken(oldRefreshToken);

            var command = new RefreshTokenCommand("expired_access_token", oldRefreshToken);

            _mockAuthenService.Setup(x => x.GetPrincipalFromExpiredToken(oldRefreshToken))
                .ReturnsAsync(Result<ClaimsPrincipal?>.Success(principal));

            _mockAccountRepository.Setup(x => x.FindByAspNetUserIdWithBlackListTokensAsync(aspNetUserId))
                .ReturnsAsync(account);

            _mockAuthenService.Setup(x => x.GenerateRefreshToken())
                .Returns(newRefreshToken);

            _mockAuthenService.Setup(x => x.GenerateAccessTokenAsync(aspNetUserId))
                .ReturnsAsync(newAccessToken);

            _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "RefreshToken.Failure");
        }
    }
}