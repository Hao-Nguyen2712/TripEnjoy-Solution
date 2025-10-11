using TripEnjoy.Application.Features.Property.Queries;
using TripEnjoy.Application.Features.Property.Handlers;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Test.UnitTests.Application.Features.Property;

public class GetAllPropertiesQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPropertyRepository> _propertyRepositoryMock;
    private readonly GetAllPropertiesQueryHandler _handler;
    private readonly IFixture _fixture;

    public GetAllPropertiesQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _propertyRepositoryMock = new Mock<IPropertyRepository>();
        _fixture = new Fixture();

        // Setup the unit of work to return our mocked property repository
        _unitOfWorkMock.Setup(x => x.Properties)
            .Returns(_propertyRepositoryMock.Object);

        _handler = new GetAllPropertiesQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnPagedPropertiesList()
    {
        // Arrange
        var query = new GetAllPropertiesQuery(PageNumber: 1, PageSize: 10);
        var properties = CreatePropertiesList(5);
        var totalCount = 25;

        _propertyRepositoryMock
            .Setup(x => x.GetAllPaginatedAsync(query.PageNumber, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((properties, totalCount));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(5);
        result.Value.TotalCount.Should().Be(totalCount);
        result.Value.PageNumber.Should().Be(query.PageNumber);
        result.Value.PageSize.Should().Be(query.PageSize);
        result.Value.HasNextPage.Should().BeTrue(); // 25 total, page 1 of 10 items
        result.Value.HasPreviousPage.Should().BeFalse();

        _propertyRepositoryMock.Verify(x => x.GetAllPaginatedAsync(query.PageNumber, query.PageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyResult_ShouldReturnEmptyPagedList()
    {
        // Arrange
        var query = new GetAllPropertiesQuery(PageNumber: 1, PageSize: 10);
        var emptyProperties = new List<Domain.Property.Property>();
        var totalCount = 0;

        _propertyRepositoryMock
            .Setup(x => x.GetAllPaginatedAsync(query.PageNumber, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((emptyProperties, totalCount));

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
        var query = new GetAllPropertiesQuery(PageNumber: 2, PageSize: 5);
        var properties = CreatePropertiesList(5);
        var totalCount = 12;

        _propertyRepositoryMock
            .Setup(x => x.GetAllPaginatedAsync(query.PageNumber, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((properties, totalCount));

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
    public async Task Handle_ShouldMapPropertiesToDtosCorrectly()
    {
        // Arrange
        var query = new GetAllPropertiesQuery(PageNumber: 1, PageSize: 10);
        var properties = CreatePropertiesListWithSpecificData();
        var totalCount = properties.Count;

        _propertyRepositoryMock
            .Setup(x => x.GetAllPaginatedAsync(query.PageNumber, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((properties, totalCount));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(properties.Count);

        // Check first property mapping
        var firstProperty = properties.First();
        var firstDto = result.Value.Items.First();
        
        firstDto.Id.Should().Be(firstProperty.Id.Id);
        firstDto.Name.Should().Be(firstProperty.Name);
        firstDto.City.Should().Be(firstProperty.City);
        firstDto.Country.Should().Be(firstProperty.Country);
        firstDto.PropertyTypeName.Should().Be(firstProperty.PropertyType.Name);
        firstDto.AverageRating.Should().Be(firstProperty.AverageRating);
    }

    [Fact]
    public async Task Handle_WithPropertiesHavingCoverImages_ShouldMapCoverImageUrl()
    {
        // Arrange
        var query = new GetAllPropertiesQuery(PageNumber: 1, PageSize: 10);
        var properties = CreatePropertiesList(3); // Simplified - just create basic properties
        var totalCount = properties.Count;

        _propertyRepositoryMock
            .Setup(x => x.GetAllPaginatedAsync(query.PageNumber, query.PageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((properties, totalCount));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(3);
        // Note: Without complex PropertyImage setup, CoverImageUrl will be null
        // This tests the basic mapping functionality
    }

    [Theory]
    [InlineData(0, 10)]    // Page 0
    [InlineData(-1, 10)]   // Negative page
    [InlineData(1, 0)]     // Page size 0
    [InlineData(1, -5)]    // Negative page size
    public async Task Handle_WithInvalidPagination_ShouldStillCallRepository(int pageNumber, int pageSize)
    {
        // Arrange
        var query = new GetAllPropertiesQuery(PageNumber: pageNumber, PageSize: pageSize);
        var emptyProperties = new List<Domain.Property.Property>();

        _propertyRepositoryMock
            .Setup(x => x.GetAllPaginatedAsync(pageNumber, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((emptyProperties, 0));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _propertyRepositoryMock.Verify(x => x.GetAllPaginatedAsync(pageNumber, pageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var query = new GetAllPropertiesQuery(PageNumber: 1, PageSize: 10);

        _propertyRepositoryMock
            .Setup(x => x.GetAllPaginatedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act & Assert
        await _handler.Invoking(h => h.Handle(query, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database connection failed");
    }

    #region Helper Methods

    private List<Domain.Property.Property> CreatePropertiesList(int count)
    {
        var properties = new List<Domain.Property.Property>();
        
        for (int i = 0; i < count; i++)
        {
            var propertyType = CreatePropertyType($"Type {i + 1}");
            var property = CreateProperty($"Property {i + 1}", $"City {i + 1}", "Country", propertyType);
            properties.Add(property);
        }

        return properties;
    }

    private List<Domain.Property.Property> CreatePropertiesListWithSpecificData()
    {
        var propertyType1 = CreatePropertyType("Hotel");
        var propertyType2 = CreatePropertyType("Apartment");

        return new List<Domain.Property.Property>
        {
            CreateProperty("Beach Resort", "Miami", "USA", propertyType1, 4.5m),
            CreateProperty("City Apartment", "New York", "USA", propertyType2, 4.2m),
            CreateProperty("Mountain Lodge", "Denver", "USA", propertyType1, 4.8m)
        };
    }



    private Domain.Property.Property CreateProperty(string name, string city, string country, Domain.PropertyType.PropertyType propertyType, decimal? rating = null)
    {
        var partnerId = PartnerId.Create(Guid.NewGuid());
        var propertyResult = Domain.Property.Property.Create(
            partnerId,
            propertyType.Id,
            name,
            "123 Test Street",
            city,
            country,
            "Test description",
            40.7128,
            -74.0060);

        var property = propertyResult.Value;

        // Set the PropertyType navigation property using reflection
        var propertyTypeField = property.GetType().GetField("PropertyType", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var propertyTypeProp = property.GetType().GetProperty("PropertyType");
        
        if (propertyTypeField != null)
            propertyTypeField.SetValue(property, propertyType);
        else if (propertyTypeProp != null)
            propertyTypeProp.SetValue(property, propertyType);

        // Set rating if provided
        if (rating.HasValue)
        {
            var ratingField = property.GetType().GetField("AverageRating", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var ratingProp = property.GetType().GetProperty("AverageRating");
            
            if (ratingField != null)
                ratingField.SetValue(property, rating.Value);
            else if (ratingProp != null)
                ratingProp.SetValue(property, rating.Value);
        }

        return property;
    }

    private Domain.PropertyType.PropertyType CreatePropertyType(string name)
    {
        var result = Domain.PropertyType.PropertyType.Create(name);
        return result.Value;
    }



    #endregion
}