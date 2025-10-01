# TripEnjoy Domain-Driven Design (DDD) Architecture Constraints

## Overview
This document defines the constraints and principles for implementing Domain-Driven Design in the TripEnjoy platform. These constraints ensure consistency, maintainability, and proper domain modeling across the solution.

## Domain Structure Constraints

### 1. Aggregate Organization
**CONSTRAINT**: Each domain aggregate MUST be organized in separate folders with clear boundaries.

**Current Structure**:
```
TripEnjoy.Domain/
├── Account/           # Account Aggregate Root
│   ├── Account.cs     # Aggregate Root
│   ├── Entities/      # Child entities
│   ├── ValueObjects/  # Value objects 
│   └── Enums/         # Domain enums
├── Property/          # Property Aggregate Root  
├── PropertyType/      # PropertyType Aggregate Root
├── AuditLog/         # AuditLog Aggregate Root
└── Common/           # Shared domain infrastructure
    ├── Models/       # Base classes
    └── Errors/       # Domain errors
```

**RULES**:
- One aggregate root per folder
- All related entities, value objects, and enums within the same folder
- No cross-aggregate direct references (use IDs only)

### 2. Base Class Hierarchy
**CONSTRAINT**: All domain objects MUST inherit from appropriate base classes.

**Hierarchy**:
```csharp
// Aggregate Roots
public class Account : AggregateRoot<AccountId>

// Entities  
public class Partner : Entity<PartnerId>
public class User : Entity<UserId>

// Value Objects
public class AccountId : ValueObject
public class PropertyId : ValueObject
```

**RULES**:
- Aggregate roots inherit from `AggregateRoot<TId>`
- Entities inherit from `Entity<TId>`
- Value objects inherit from `ValueObject`
- All IDs are strongly-typed value objects

## Value Object Constraints

### 3. Strongly-Typed IDs
**CONSTRAINT**: All entity identifiers MUST be strongly-typed value objects.

**Implementation Pattern**:
```csharp
public class AccountId : ValueObject
{
    public Guid Id { get; private set; }

    public AccountId(Guid id) => Id = id;

    // REQUIRED: Factory methods
    public static AccountId Create(Guid id) => new(id);
    public static AccountId CreateUnique() => new(Guid.NewGuid());

    // REQUIRED: Equality implementation
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
```

**RULES**:
- Private setter for Id property
- Both `Create(Guid id)` and `CreateUnique()` factory methods
- Proper equality implementation via `GetEqualityComponents()`
- No public parameterless constructor

### 4. Value Object Immutability  
**CONSTRAINT**: Value objects MUST be immutable after creation.

**RULES**:
- All properties have private setters
- No mutating methods
- Construction only through factory methods
- Implement structural equality via `GetEqualityComponents()`

## Entity Constraints

### 5. Entity Identity and Encapsulation
**CONSTRAINT**: Entities MUST encapsulate their state and expose behavior through methods.

**Implementation Pattern**:
```csharp
public class Account : AggregateRoot<AccountId>
{
    // REQUIRED: Private setters
    public string AccountEmail { get; private set; }
    public string Status { get; private set; }
    
    // REQUIRED: EF constructor  
    private Account() : base(AccountId.CreateUnique()) { /* EF setup */ }
    
    // REQUIRED: Public constructor with validation
    public Account(AccountId id, string aspNetUserId, string accountEmail) : base(id)
    {
        // Validation and assignment
    }
    
    // REQUIRED: Static factory method with validation
    public static Result<Account> Create(string aspNetUserId, string accountEmail)
    {
        // Validation logic
        if (!isValidEmail) return Result<Account>.Failure(DomainError.Account.InvalidEmail);
        return Result<Account>.Success(new Account(AccountId.CreateUnique(), aspNetUserId, accountEmail));
    }
}
```

**RULES**:
- Private setters for all properties
- Private parameterless constructor for EF Core
- Public constructor for valid object creation
- Static `Create()` factory method with validation
- Return `Result<T>` from factory methods

### 6. Collection Encapsulation
**CONSTRAINT**: Entity collections MUST be properly encapsulated.

**Pattern**:
```csharp
public class Account : AggregateRoot<AccountId>
{
    // REQUIRED: Private backing field
    private readonly List<RefreshToken> _refreshTokens = new();
    
    // REQUIRED: Read-only public exposure
    public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
    
    // REQUIRED: Behavior methods for collection manipulation
    public Result<RefreshToken> AddRefreshToken(string token)
    {
        var refreshToken = RefreshToken.Create(this.Id, token);
        _refreshTokens.Add(refreshToken);
        UpdatedAt = DateTime.UtcNow;
        return Result<RefreshToken>.Success(refreshToken);
    }
}
```

**RULES**:
- Private `List<T>` backing fields
- Public `IReadOnlyList<T>` properties  
- Collection manipulation through domain methods only
- Always update timestamps when modifying collections

## Business Logic Constraints

### 7. Domain Validation and Business Rules
**CONSTRAINT**: All business logic and validation MUST reside in the domain layer.

**Implementation Pattern**:
```csharp
public Result MarkAsActive()
{
    // REQUIRED: Validate current state
    if (Status != AccountStatusEnum.PendingVerification.ToString())
    {
        return Result.Failure(DomainError.Account.AlreadyActivated);
    }
    
    // REQUIRED: Apply business rule
    Status = AccountStatusEnum.Active.ToString();
    UpdatedAt = DateTime.UtcNow;
    
    return Result.Success();
}

public static Result<Account> Create(string aspNetUserId, string accountEmail)
{
    // REQUIRED: Input validation
    var expression = new EmailAddressAttribute();
    if (!expression.IsValid(accountEmail))
    {
        return Result<Account>.Failure(DomainError.Account.InvalidEmail);
    }
    
    return Result<Account>.Success(new Account(AccountId.CreateUnique(), aspNetUserId, accountEmail));
}
```

**RULES**:
- Validate state before operations
- Use domain errors from `DomainError` static class
- Return `Result<T>` or `Result` from all operations
- Update timestamps for state changes
- No throwing exceptions from domain methods

### 8. Domain Error Handling
**CONSTRAINT**: Domain errors MUST be centralized and typed.

**Pattern**:
```csharp
public static class DomainError
{
    public static class Account
    {
        public static readonly Error InvalidEmail = new(
            "Account.InvalidEmail",
            "The email format is invalid.",
            ErrorType.Validation);
            
        public static readonly Error AlreadyActivated = new(
            "Account.AlreadyActivated", 
            "The account has already been activated.",
            ErrorType.Failure);
    }
}
```

**RULES**:
- Organized by aggregate in static nested classes
- Consistent naming: `{Aggregate}.{ErrorName}`
- Descriptive error messages
- Appropriate `ErrorType` classification
- No domain exceptions - use `Result<T>` pattern

## Aggregate Constraints

### 9. Aggregate Boundaries
**CONSTRAINT**: Aggregates MUST maintain clear boundaries and consistency.

**RULES**:
- One aggregate root per aggregate
- Child entities accessed only through aggregate root
- No direct references between aggregates (use IDs)
- Aggregate roots manage all internal consistency
- Transactions should not span multiple aggregates

### 10. Aggregate Root Responsibilities
**CONSTRAINT**: Aggregate roots MUST control access to all internal entities.

**Pattern**:
```csharp
public class Account : AggregateRoot<AccountId>  
{
    // REQUIRED: Manage child entities
    public User? User { get; private set; }
    public Partner? Partner { get; private set; }
    
    // REQUIRED: Factory methods for child entities
    public Result AddUserInformation(string fullName)
    {
        var userResult = Domain.Account.Entities.User.Create(this.Id, fullName);
        if (userResult.IsFailure) return Result.Failure(userResult.Errors);
        
        User = userResult.Value;
        return Result.Success();
    }
}
```

**RULES**:
- Private setters for child entity properties
- Child entity creation through aggregate root methods
- Validate child entity invariants in aggregate root
- Coordinate child entity state changes

## Infrastructure Integration Constraints

### 11. Entity Framework Integration
**CONSTRAINT**: Domain entities MUST support EF Core while maintaining encapsulation.

**Required Elements**:
```csharp
public class Account : AggregateRoot<AccountId>
{
    // REQUIRED: Private parameterless constructor for EF
    private Account() : base(AccountId.CreateUnique())
    {
        // Initialize non-nullable properties for EF
        AspNetUserId = null!;
        AccountEmail = null!;
        Status = null!;
    }
    
    // REQUIRED: Proper navigation properties
    public Partner? Partner { get; private set; }
    public User? User { get; private set; }
}
```

**RULES**:
- Private parameterless constructor with `null!` for non-nullable reference types
- Navigation properties with private setters
- Backing collections initialized in constructor
- Value object conversions handled in persistence layer

### 12. Domain Event Constraints
**CONSTRAINT**: Domain events SHOULD be raised from aggregate roots (when implemented).

**Future Pattern**:
```csharp
public class Account : AggregateRoot<AccountId>
{
    public Result MarkAsActive()
    {
        if (Status != AccountStatusEnum.PendingVerification.ToString())
            return Result.Failure(DomainError.Account.AlreadyActivated);
            
        Status = AccountStatusEnum.Active.ToString(); 
        UpdatedAt = DateTime.UtcNow;
        
        // FUTURE: Raise domain event
        // RaiseDomainEvent(new AccountActivatedEvent(Id));
        
        return Result.Success();
    }
}
```

## Testing Constraints

### 13. Domain Unit Testing
**CONSTRAINT**: Domain logic MUST be thoroughly unit tested.

**Test Categories**:
- Factory method validation
- Business rule enforcement  
- State transition validation
- Collection manipulation
- Error condition handling

**Pattern**:
```csharp
[Test]
public void Create_WithInvalidEmail_ShouldReturnFailure()
{
    // Arrange
    var invalidEmail = "invalid-email";
    
    // Act  
    var result = Account.Create("userId", invalidEmail);
    
    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(DomainError.Account.InvalidEmail);
}
```

## Migration and Evolution Constraints

### 14. Domain Model Evolution
**CONSTRAINT**: Domain model changes MUST maintain backward compatibility.

**RULES**:
- Additive changes preferred (new optional properties)
- Breaking changes require migration strategy
- Preserve existing factory method signatures
- Version domain events when structure changes

### 15. Persistence Compatibility  
**CONSTRAINT**: Domain changes MUST consider persistence implications.

**RULES**:
- Value object changes need persistence layer updates
- Collection changes may require migration scripts
- Property renames need database migration
- Always test EF Core mapping after domain changes

## Validation Checklist

When implementing new domain features, ensure:

- [ ] Aggregate boundaries are clear and respected
- [ ] All IDs are strongly-typed value objects  
- [ ] Factory methods return `Result<T>` with validation
- [ ] Business rules enforced in domain methods
- [ ] Collections properly encapsulated
- [ ] Domain errors centralized in `DomainError`
- [ ] EF Core compatibility maintained
- [ ] Comprehensive unit tests written
- [ ] No cross-aggregate direct references
- [ ] State transitions properly validated

## Anti-Patterns to Avoid

❌ **DON'T**:
- Expose public setters on domain properties
- Create domain entities with `new` operator outside factories
- Throw exceptions from domain methods
- Reference other aggregates directly
- Put business logic in application services
- Use primitive types for entity IDs
- Allow invalid state in domain objects
- Bypass aggregate roots to access child entities

✅ **DO**:
- Use factory methods with validation
- Return `Result<T>` from domain operations
- Encapsulate collections with backing fields
- Validate business rules in domain methods
- Use strongly-typed IDs as value objects
- Maintain aggregate consistency
- Centralize domain errors
- Test all business rules thoroughly

---

This document serves as the canonical reference for DDD implementation in TripEnjoy. All domain layer development should adhere to these constraints to ensure consistency and maintainability.