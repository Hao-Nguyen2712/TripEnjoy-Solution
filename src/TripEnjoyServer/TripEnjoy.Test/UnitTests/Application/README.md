# TripEnjoy Application Layer Unit Tests

This directory contains comprehensive unit tests for the TripEnjoy Application layer.

## Test Structure

```
UnitTests/
└── Application/
    └── Features/
        ├── Authentication/
        │   ├── RegisterUserCommandHandlerTests.cs
        │   └── LoginStepOneCommandHandlerTests.cs
        └── Property/
            ├── CreatePropertyCommandHandlerTests.cs
            └── GetAllPropertiesQueryHandlerTests.cs
```

## Running Tests

### Prerequisites

Ensure you have installed the required NuGet packages:

```bash
dotnet add package Moq --version 4.20.69
dotnet add package FluentAssertions --version 6.12.0
dotnet add package AutoFixture --version 4.18.0
dotnet add package AutoFixture.Xunit2 --version 4.18.0
dotnet add package Microsoft.Extensions.Logging.Abstractions --version 8.0.0
```

### Run All Tests

```bash
# From the test project directory
dotnet test

# From the solution root
dotnet test src/TripEnjoyServer/TripEnjoy.Test/TripEnjoy.Test.csproj
```

### Run Specific Test Categories

```bash
# Run only Application layer tests
dotnet test --filter "FullyQualifiedName~UnitTests.Application"

# Run only Authentication tests
dotnet test --filter "FullyQualifiedName~Authentication"

# Run only Property tests
dotnet test --filter "FullyQualifiedName~Property"

# Run specific test class
dotnet test --filter "FullyQualifiedName~RegisterUserCommandHandlerTests"
```

### Run with Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run with Detailed Output

```bash
dotnet test --verbosity normal
```

## Test Coverage

### Authentication Features
- ✅ `RegisterUserCommandHandler` - User registration with validation
- ✅ `LoginStepOneCommandHandler` - First step of two-factor authentication

### Property Features  
- ✅ `CreatePropertyCommandHandler` - Property creation with partner validation
- ✅ `GetAllPropertiesQueryHandler` - Paginated property retrieval with DTO mapping

## Test Patterns Used

### 1. **Arrange-Act-Assert Pattern**
All tests follow the AAA pattern for clarity and consistency.

### 2. **Mocking External Dependencies**
- Repository interfaces (`IUnitOfWork`, `IGenericRepository<T>`)
- External services (`IAuthenService`, `IEmailService`)
- Infrastructure concerns (`IHttpContextAccessor`, `ILogger<T>`)

### 3. **Result Pattern Validation**
Tests validate both success and failure scenarios using TripEnjoy's `Result<T>` pattern.

### 4. **Edge Case Testing**
- Invalid inputs
- Null/empty values
- Boundary conditions
- Exception scenarios

### 5. **Behavior Verification**
- Method call verification
- Parameter validation  
- Interaction testing

## Key Testing Principles

### ✅ **Unit Test Characteristics**
- **Fast**: No external dependencies (database, network, file system)
- **Isolated**: Each test is independent and can run in any order
- **Repeatable**: Same results every time
- **Self-validating**: Clear pass/fail without manual inspection
- **Timely**: Written alongside or before the production code

### ✅ **Mocking Strategy**
- Mock all external dependencies
- Use `Mock<T>` for interface mocking
- Setup specific scenarios for different test cases
- Verify interactions where behavior matters

### ✅ **Test Organization**
- One test class per handler
- Descriptive test method names
- Group related tests with `#region` blocks
- Helper methods for test data creation

## Example Test Structure

```csharp
public class ExampleCommandHandlerTests
{
    private readonly Mock<IDependency> _dependencyMock;
    private readonly ExampleCommandHandler _handler;
    private readonly IFixture _fixture;

    public ExampleCommandHandlerTests()
    {
        _dependencyMock = new Mock<IDependency>();
        _fixture = new Fixture();
        _handler = new ExampleCommandHandler(_dependencyMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidInput_ShouldReturnSuccess()
    {
        // Arrange
        var command = CreateValidCommand();
        SetupMockForSuccess();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        VerifyExpectedInteractions();
    }

    [Fact]
    public async Task Handle_WithInvalidInput_ShouldReturnFailure()
    {
        // Arrange
        var command = CreateInvalidCommand();
        SetupMockForFailure();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainExpectedError();
    }
}
```

## Best Practices Followed

### 1. **Test Naming**
- Method name describes the scenario: `Handle_WithValidInput_ShouldReturnSuccess`
- Test class names end with `Tests`
- Clear, descriptive assertions

### 2. **Test Data**
- Use AutoFixture for generating test data
- Create helper methods for complex object creation
- Use constants for important test values

### 3. **Assertions**
- Use FluentAssertions for readable assertions
- Test both positive and negative scenarios
- Verify side effects and interactions

### 4. **Maintainability**
- Keep tests simple and focused
- Minimize test setup complexity
- Use descriptive variable names

## Continuous Integration

These tests are designed to run in CI/CD pipelines:

```yaml
# Example GitHub Actions step
- name: Run Unit Tests
  run: dotnet test --configuration Release --no-build --verbosity normal --collect:"XPlat Code Coverage"
```

## Contributing

When adding new Application layer features:

1. Create corresponding test files following the established pattern
2. Test both success and failure scenarios
3. Mock all external dependencies
4. Follow the AAA pattern
5. Use descriptive test names
6. Verify interactions with mocked dependencies
7. Test edge cases and boundary conditions

The unit tests serve as both validation and documentation of the expected behavior of your Application layer handlers.