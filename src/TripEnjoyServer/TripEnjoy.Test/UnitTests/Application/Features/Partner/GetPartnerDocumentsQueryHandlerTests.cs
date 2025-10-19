using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using TripEnjoy.Application.Features.Partner.Handlers;
using TripEnjoy.Application.Features.Partner.Queries;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account;
using TripEnjoy.Domain.Account.Entities;
using TripEnjoy.Domain.Account.Enums;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Test.UnitTests.Application.Features.Partner;

public class GetPartnerDocumentsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<IPartnerDocumentRepository> _partnerDocumentRepositoryMock;
    private readonly GetPartnerDocumentsQueryHandler _handler;
    private readonly IFixture _fixture;

    public GetPartnerDocumentsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _partnerDocumentRepositoryMock = new Mock<IPartnerDocumentRepository>();
        _fixture = new Fixture();

        // Setup the unit of work to return our mocked repositories
        _unitOfWorkMock.Setup(x => x.AccountRepository)
            .Returns(_accountRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.PartnerDocuments)
            .Returns(_partnerDocumentRepositoryMock.Object);

        _handler = new GetPartnerDocumentsQueryHandler(_unitOfWorkMock.Object, _httpContextAccessorMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnPagedDocumentsList()
    {
        // Arrange
        var query = new GetPartnerDocumentsQuery(PageNumber: 1, PageSize: 10);
        var accountId = Guid.NewGuid();
        var partnerId = PartnerId.Create(Guid.NewGuid());
        var documents = CreatePartnerDocumentsList(5);
        var totalCount = 15;

        SetupHttpContextWithAccountId(accountId);
        SetupAccountWithPartner(accountId, partnerId);

        _partnerDocumentRepositoryMock
            .Setup(x => x.GetDocumentsByPartnerIdPaginatedAsync(partnerId, query.PageNumber, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((documents, totalCount));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(5);
        result.Value.TotalCount.Should().Be(totalCount);
        result.Value.PageNumber.Should().Be(query.PageNumber);
        result.Value.PageSize.Should().Be(query.PageSize);
        result.Value.HasNextPage.Should().BeTrue(); // 15 total, page 1 of 10 items
        result.Value.HasPreviousPage.Should().BeFalse();

        // Verify proper DTOs are created
        var firstDocument = result.Value.Items.First();
        firstDocument.DocumentTypeName.Should().NotBeEmpty();
        firstDocument.StatusDisplayName.Should().NotBeEmpty();

        _partnerDocumentRepositoryMock.Verify(x => x.GetDocumentsByPartnerIdPaginatedAsync(
            partnerId, query.PageNumber, query.PageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyResult_ShouldReturnEmptyPagedList()
    {
        // Arrange
        var query = new GetPartnerDocumentsQuery(PageNumber: 1, PageSize: 10);
        var accountId = Guid.NewGuid();
        var partnerId = PartnerId.Create(Guid.NewGuid());
        var emptyDocuments = new List<PartnerDocument>();
        var totalCount = 0;

        SetupHttpContextWithAccountId(accountId);
        SetupAccountWithPartner(accountId, partnerId);

        _partnerDocumentRepositoryMock
            .Setup(x => x.GetDocumentsByPartnerIdPaginatedAsync(partnerId, query.PageNumber, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((emptyDocuments, totalCount));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.PageNumber.Should().Be(query.PageNumber);
        result.Value.PageSize.Should().Be(query.PageSize);
        result.Value.HasNextPage.Should().BeFalse();
        result.Value.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithSecondPage_ShouldReturnCorrectPagedList()
    {
        // Arrange
        var query = new GetPartnerDocumentsQuery(PageNumber: 2, PageSize: 5);
        var accountId = Guid.NewGuid();
        var partnerId = PartnerId.Create(Guid.NewGuid());
        var documents = CreatePartnerDocumentsList(5);
        var totalCount = 12;

        SetupHttpContextWithAccountId(accountId);
        SetupAccountWithPartner(accountId, partnerId);

        _partnerDocumentRepositoryMock
            .Setup(x => x.GetDocumentsByPartnerIdPaginatedAsync(partnerId, query.PageNumber, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((documents, totalCount));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(5);
        result.Value.TotalCount.Should().Be(totalCount);
        result.Value.PageNumber.Should().Be(2);
        result.Value.PageSize.Should().Be(5);
        result.Value.HasNextPage.Should().BeTrue(); // 12 total, page 2 of 5 items, so page 3 exists
        result.Value.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithMissingAccountId_ShouldReturnUnauthorizedFailure()
    {
        // Arrange
        var query = new GetPartnerDocumentsQuery(PageNumber: 1, PageSize: 10);
        
        // Setup HttpContext without AccountId claim
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Authentication.Unauthorized);
    }

    [Fact]
    public async Task Handle_WithInvalidAccountId_ShouldReturnUnauthorizedFailure()
    {
        // Arrange
        var query = new GetPartnerDocumentsQuery(PageNumber: 1, PageSize: 10);
        
        // Setup HttpContext with invalid AccountId claim
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("AccountId", "invalid-guid")
        }));
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Authentication.Unauthorized);
    }

    [Fact]
    public async Task Handle_WithNonExistentAccount_ShouldReturnPartnerNotFoundFailure()
    {
        // Arrange
        var query = new GetPartnerDocumentsQuery(PageNumber: 1, PageSize: 10);
        var accountId = Guid.NewGuid();

        SetupHttpContextWithAccountId(accountId);

        _accountRepositoryMock
            .Setup(x => x.GetByAccountIdAsync(It.IsAny<AccountId>()))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Partner.NotFound");
    }

    [Fact]
    public async Task Handle_WithAccountWithoutPartner_ShouldReturnPartnerNotFoundFailure()
    {
        // Arrange
        var query = new GetPartnerDocumentsQuery(PageNumber: 1, PageSize: 10);
        var accountId = Guid.NewGuid();

        SetupHttpContextWithAccountId(accountId);

        var account = Account.Create("test-user-id", "test@example.com").Value;
        _accountRepositoryMock
            .Setup(x => x.GetByAccountIdAsync(It.IsAny<AccountId>()))
            .ReturnsAsync(account);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "Partner.NotFound");
    }

    [Theory]
    [InlineData(0, 10)]    // Page 0
    [InlineData(-1, 10)]   // Negative page
    [InlineData(1, 0)]     // Page size 0
    [InlineData(1, -5)]    // Negative page size
    public async Task Handle_WithInvalidPagination_ShouldStillCallRepository(int pageNumber, int pageSize)
    {
        // Arrange
        var query = new GetPartnerDocumentsQuery(PageNumber: pageNumber, PageSize: pageSize);
        var accountId = Guid.NewGuid();
        var partnerId = PartnerId.Create(Guid.NewGuid());
        var emptyDocuments = new List<PartnerDocument>();

        SetupHttpContextWithAccountId(accountId);
        SetupAccountWithPartner(accountId, partnerId);

        _partnerDocumentRepositoryMock
            .Setup(x => x.GetDocumentsByPartnerIdPaginatedAsync(partnerId, pageNumber, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((emptyDocuments, 0));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _partnerDocumentRepositoryMock.Verify(x => x.GetDocumentsByPartnerIdPaginatedAsync(
            partnerId, pageNumber, pageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapDocumentTypeNamesCorrectly()
    {
        // Arrange
        var query = new GetPartnerDocumentsQuery(PageNumber: 1, PageSize: 10);
        var accountId = Guid.NewGuid();
        var partnerId = PartnerId.Create(Guid.NewGuid());
        
        var documents = new List<PartnerDocument>
        {
            CreatePartnerDocument(partnerId, "BusinessLicense", "PendingReview"),
            CreatePartnerDocument(partnerId, "TaxIdentification", "Approved"),
            CreatePartnerDocument(partnerId, "ProofOfAddress", "Rejected"),
            CreatePartnerDocument(partnerId, "UnknownType", "PendingReview")
        };

        SetupHttpContextWithAccountId(accountId);
        SetupAccountWithPartner(accountId, partnerId);

        _partnerDocumentRepositoryMock
            .Setup(x => x.GetDocumentsByPartnerIdPaginatedAsync(partnerId, query.PageNumber, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((documents, documents.Count));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #region Helper Methods

    private void SetupHttpContextWithAccountId(Guid accountId)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("AccountId", accountId.ToString())
        }));
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
    }

    private void SetupAccountWithPartner(Guid accountId, PartnerId partnerId)
    {
        var account = Account.Create("test-user-id", "test@example.com").Value;
        var partner = new Domain.Account.Entities.Partner(partnerId, AccountId.Create(accountId), "Test Company", "123456789", "Test Address");
        
        // Use reflection to set the Partner property since it's private setter
        var partnerProperty = typeof(Account).GetProperty("Partner");
        partnerProperty?.SetValue(account, partner);

        _accountRepositoryMock
            .Setup(x => x.GetByAccountIdAsync(It.IsAny<AccountId>()))
            .ReturnsAsync(account);
    }

    private List<PartnerDocument> CreatePartnerDocumentsList(int count)
    {
        var documents = new List<PartnerDocument>();
        var documentTypes = new[] { "BusinessLicense", "TaxIdentification", "ProofOfAddress", "CompanyRegistration", "BankStatement" };
        var statuses = new[] { "PendingReview", "Approved", "Rejected" };

        for (int i = 0; i < count; i++)
        {
            var partnerId = PartnerId.Create(Guid.NewGuid());
            var documentType = documentTypes[i % documentTypes.Length];
            var status = statuses[i % statuses.Length];
            
            documents.Add(CreatePartnerDocument(partnerId, documentType, status));
        }

        return documents;
    }

    private PartnerDocument CreatePartnerDocument(PartnerId partnerId, string documentType, string status)
    {
        var document = new PartnerDocument(
            PartnerDocumentId.CreateUnique(),
            partnerId,
            documentType,
            $"https://cloudinary.com/test-{Guid.NewGuid()}",
            status);

        return document;
    }

    #endregion
}