# TripEnjoy Architecture Diagrams

This document provides visual representations of the TripEnjoy platform architecture using ASCII diagrams and mermaid syntax.

---

## Table of Contents

1. [Solution Architecture](#1-solution-architecture)
2. [Clean Architecture Layers](#2-clean-architecture-layers)
3. [Request Flow (CQRS Pipeline)](#3-request-flow-cqrs-pipeline)
4. [Authentication Flow](#4-authentication-flow)
5. [Domain Model](#5-domain-model)
6. [Database Schema](#6-database-schema)
7. [API Structure](#7-api-structure)
8. [Deployment Architecture](#8-deployment-architecture)

---

## 1. Solution Architecture

### High-Level Project Structure

```
┌─────────────────────────────────────────────────────────────────────┐
│                        TripEnjoy Solution                            │
│                       (.NET 8 Platform)                              │
└─────────────────────────────────────────────────────────────────────┘
                                  │
                 ┌────────────────┴────────────────┐
                 │                                  │
        ┌────────▼────────┐              ┌────────▼────────┐
        │   Presentation  │              │   Infrastructure │
        │      Layer      │              │      Layer      │
        └────────┬────────┘              └────────┬────────┘
                 │                                │
    ┌────────────┼────────────┐      ┌───────────┼──────────┐
    │            │            │      │           │          │
┌───▼───┐   ┌───▼───┐   ┌───▼───┐ ┌▼──────┐ ┌──▼───────┐  │
│  API  │   │Client │   │  Test │ │Infra  │ │  Infra.  │  │
│       │   │(MVC)  │   │       │ │       │ │Persistence│  │
└───┬───┘   └───┬───┘   └───────┘ └───┬───┘ └──────┬───┘  │
    │           │                      │            │      │
    │           │                      │            │      │
    └───────────┴──────────┬───────────┴────────────┘      │
                           │                                │
                  ┌────────▼────────┐                       │
                  │   Application   │                       │
                  │      Layer      │                       │
                  │   (CQRS/MediatR)│                       │
                  └────────┬────────┘                       │
                           │                                │
                  ┌────────▼────────┐                       │
                  │     Domain      │                       │
                  │      Layer      │                       │
                  │   (DDD/Entities)│                       │
                  └─────────────────┘                       │
                           │                                │
                  ┌────────▼────────┐                       │
                  │   ShareKernel   │◄──────────────────────┘
                  │  (Common/DTOs)  │
                  └─────────────────┘
```

### Project Dependencies

```
TripEnjoy.Api
 ├── TripEnjoy.Application
 │    ├── TripEnjoy.Domain
 │    │    └── TripEnjoy.ShareKernel
 │    └── TripEnjoy.ShareKernel
 ├── TripEnjoy.Infrastructure
 │    ├── TripEnjoy.Application
 │    └── TripEnjoy.ShareKernel
 └── TripEnjoy.Infrastructure.Persistence
      ├── TripEnjoy.Domain
      └── TripEnjoy.ShareKernel

TripEnjoy.Client
 └── TripEnjoy.ShareKernel

TripEnjoy.Test
 ├── TripEnjoy.Api
 ├── TripEnjoy.Application
 ├── TripEnjoy.Domain
 └── TripEnjoy.Infrastructure.Persistence
```

---

## 2. Clean Architecture Layers

### Onion Architecture View

```
                    ┌───────────────────────────────────────┐
                    │                                       │
                    │    ┌───────────────────────────┐     │
                    │    │                           │     │
                    │    │   ┌───────────────────┐   │     │
                    │    │   │                   │   │     │
                    │    │   │   ┌───────────┐   │   │     │
                    │    │   │   │           │   │   │     │
                    │    │   │   │  Domain   │   │   │     │
                    │    │   │   │   Layer   │   │   │     │
                    │    │   │   │           │   │   │     │
                    │    │   │   │ • Entities│   │   │     │
                    │    │   │   │ • VOs     │   │   │     │
                    │    │   │   │ • Aggrs   │   │   │     │
                    │    │   │   │ • Errors  │   │   │     │
                    │    │   │   └───────────┘   │   │     │
                    │    │   │                   │   │     │
                    │    │   │   Application     │   │     │
                    │    │   │      Layer        │   │     │
                    │    │   │                   │   │     │
                    │    │   │ • CQRS Handlers   │   │     │
                    │    │   │ • Validators      │   │     │
                    │    │   │ • Behaviors       │   │     │
                    │    │   │ • Interfaces      │   │     │
                    │    │   └───────────────────┘   │     │
                    │    │                           │     │
                    │    │   Infrastructure Layer    │     │
                    │    │                           │     │
                    │    │ • Repositories            │     │
                    │    │ • EF Core DbContext       │     │
                    │    │ • External Services       │     │
                    │    │   - Email                 │     │
                    │    │   - Cloudinary            │     │
                    │    │   - Redis Cache           │     │
                    │    └───────────────────────────┘     │
                    │                                       │
                    │    Presentation Layer                 │
                    │                                       │
                    │  • REST API Controllers               │
                    │  • MVC Controllers                    │
                    │  • Middleware                         │
                    │  • Views (Razor)                      │
                    └───────────────────────────────────────┘

                    Dependencies Flow: Inward Only →
```

### Layer Responsibilities

```
┌──────────────────────────────────────────────────────────────────┐
│ PRESENTATION LAYER (API + Client)                                │
├──────────────────────────────────────────────────────────────────┤
│ Responsibilities:                                                 │
│ • HTTP request/response handling                                  │
│ • Input validation (controller level)                             │
│ • Authentication/Authorization                                    │
│ • API versioning and documentation                                │
│ • View rendering (MVC)                                            │
│                                                                   │
│ Dependencies: Application, Infrastructure                         │
└──────────────────────────────────────────────────────────────────┘
                                  ↓
┌──────────────────────────────────────────────────────────────────┐
│ APPLICATION LAYER                                                 │
├──────────────────────────────────────────────────────────────────┤
│ Responsibilities:                                                 │
│ • Use case orchestration                                          │
│ • CQRS command/query handling                                     │
│ • Business flow coordination                                      │
│ • Input validation (FluentValidation)                             │
│ • Cross-cutting concerns (logging, validation behaviors)          │
│                                                                   │
│ Dependencies: Domain                                              │
└──────────────────────────────────────────────────────────────────┘
                                  ↓
┌──────────────────────────────────────────────────────────────────┐
│ DOMAIN LAYER                                                      │
├──────────────────────────────────────────────────────────────────┤
│ Responsibilities:                                                 │
│ • Business logic and rules                                        │
│ • Aggregate roots and entities                                    │
│ • Value objects (strongly-typed IDs)                              │
│ • Domain events (foundation)                                      │
│ • Domain errors                                                   │
│                                                                   │
│ Dependencies: None (Pure business logic)                          │
└──────────────────────────────────────────────────────────────────┘
                                  ↑
┌──────────────────────────────────────────────────────────────────┐
│ INFRASTRUCTURE LAYER                                              │
├──────────────────────────────────────────────────────────────────┤
│ Responsibilities:                                                 │
│ • Data persistence (EF Core)                                      │
│ • External service integration                                    │
│ • Caching (Redis)                                                 │
│ • Email sending (SMTP)                                            │
│ • Image storage (Cloudinary)                                      │
│ • Background jobs (Hangfire)                                      │
│                                                                   │
│ Dependencies: Application, Domain                                 │
└──────────────────────────────────────────────────────────────────┘
```

---

## 3. Request Flow (CQRS Pipeline)

### Complete Request Lifecycle

```
┌──────────────┐
│   HTTP       │
│   Client     │
└──────┬───────┘
       │
       │ POST /api/v1/properties
       │ { name, address, ... }
       ▼
┌─────────────────────────────────────────────────────────────┐
│  API CONTROLLER (Presentation Layer)                        │
│  PropertiesController.CreateProperty()                      │
├─────────────────────────────────────────────────────────────┤
│  1. Bind request to CreatePropertyCommand                   │
│  2. Call _sender.Send(command)                              │
│  3. Return HandleResult(result)                             │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           │ Send Command
                           ▼
┌─────────────────────────────────────────────────────────────┐
│  MEDIATR PIPELINE (Application Layer)                       │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌────────────────────────────────────────────┐             │
│  │ Behavior 1: ValidationBehavior             │             │
│  │ • Collect all IValidator<T> instances      │             │
│  │ • Run FluentValidation rules               │             │
│  │ • Return Result.Failure if validation fails│             │
│  └────────────────┬───────────────────────────┘             │
│                   │ ✓ Valid                                 │
│                   ▼                                         │
│  ┌────────────────────────────────────────────┐             │
│  │ Behavior 2: LoggingBehavior (future)       │             │
│  │ • Log request details                      │             │
│  │ • Track execution time                     │             │
│  └────────────────┬───────────────────────────┘             │
│                   │                                         │
│                   ▼                                         │
│  ┌────────────────────────────────────────────┐             │
│  │ HANDLER: CreatePropertyCommandHandler      │             │
│  │                                            │             │
│  │  1. Extract PartnerId from claims          │             │
│  │  2. Validate PropertyType exists           │             │
│  │  3. Call Domain factory method             │             │
│  │  4. Add entity to repository               │             │
│  │  5. Save via UnitOfWork                    │             │
│  │  6. Return Result<PropertyId>              │             │
│  └────────────────┬───────────────────────────┘             │
│                   │                                         │
└───────────────────┼─────────────────────────────────────────┘
                    │
                    │ Domain Operation
                    ▼
┌─────────────────────────────────────────────────────────────┐
│  DOMAIN LAYER                                                │
├─────────────────────────────────────────────────────────────┤
│  Property.Create(PropertyId.CreateUnique(), partnerId, ...)  │
│                                                              │
│  • Validate business rules                                  │
│  • Create Property aggregate                                │
│  • Initialize collections                                   │
│  • Set default values                                       │
│  • Return Result<Property>                                  │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       │ Persist
                       ▼
┌─────────────────────────────────────────────────────────────┐
│  INFRASTRUCTURE LAYER                                        │
├─────────────────────────────────────────────────────────────┤
│  UnitOfWork.Properties.AddAsync(property)                   │
│  UnitOfWork.SaveChangesAsync()                              │
│                                                              │
│  • Convert value objects to database types                  │
│  • Execute SQL INSERT statement                             │
│  • Return saved entity                                      │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       │ Result<PropertyId>
                       ▼
┌─────────────────────────────────────────────────────────────┐
│  API CONTROLLER                                              │
│  HandleResult(result, "Property created successfully")       │
├─────────────────────────────────────────────────────────────┤
│  if (result.IsSuccess)                                       │
│    return Ok(new ApiResponse {                              │
│      Success = true,                                        │
│      Data = result.Value,                                   │
│      Message = "Property created successfully"              │
│    });                                                      │
│  else                                                       │
│    return MapErrorToHttpStatus(result.Errors)               │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       │ HTTP 200 OK
                       │ { success: true, data: { propertyId } }
                       ▼
┌──────────────┐
│   HTTP       │
│   Client     │
└──────────────┘
```

### MediatR Pipeline Details

```
IRequest<Result<T>>
       │
       ▼
┌─────────────────────────────────────────────────────────────┐
│                    IPipelineBehavior Chain                   │
└─────────────────────────────────────────────────────────────┘
       │
       ├──> ValidationBehavior<TRequest, TResponse>
       │     │
       │     ├─> Get IValidator<TRequest>[] from DI
       │     ├─> context.ValidateAsync(request)
       │     ├─> Aggregate all failures
       │     └─> Return Result.Failure(errors) if any
       │
       ├──> LoggingBehavior<TRequest, TResponse> (future)
       │     │
       │     ├─> Log request start
       │     ├─> Call next()
       │     ├─> Log request end + duration
       │     └─> Return response
       │
       └──> CachingBehavior<TRequest, TResponse> (future)
             │
             ├─> Check cache for response
             ├─> Return cached if exists
             ├─> Call next() if not cached
             └─> Store response in cache
       │
       ▼
IRequestHandler<TRequest, TResponse>
       │
       └──> Handle(TRequest, CancellationToken)
```

---

## 4. Authentication Flow

### Two-Factor Login Process

```
┌────────────┐                                                    ┌─────────────┐
│   Client   │                                                    │  Email      │
│  (Browser) │                                                    │  Service    │
└──────┬─────┘                                                    └──────▲──────┘
       │                                                                 │
       │ 1. POST /api/v1/auth/login-user-step-one                      │
       │    { email, password }                                         │
       ▼                                                                │
┌─────────────────────────────────────────────────────────┐            │
│  AuthController.LoginUserStepOne()                      │            │
└──────────────────────────┬──────────────────────────────┘            │
                           │                                           │
                           │ Send Command                              │
                           ▼                                           │
┌─────────────────────────────────────────────────────────┐            │
│  LoginUserStepOneCommandHandler                         │            │
├─────────────────────────────────────────────────────────┤            │
│  1. Get user by email (ASP.NET Identity)                │            │
│  2. Check password validity                             │            │
│  3. Verify user has "User" role                         │            │
│  4. Generate 6-digit OTP code                           │            │
│  5. Store OTP in Redis (5 min TTL)                      │            │
│  6. Send email with OTP ──────────────────────────────────────────> │
│  7. Return Result.Success()                             │            │
└──────────────────────────┬──────────────────────────────┘            │
                           │                                           │
                           │ Result.Success()                          │
                           ▼                                           │
┌────────────┐                                                         │
│   Client   │                                                         │
│            │◄───── 200 OK { success: true } ────────────────────────┘
│            │
│  Display   │
│  OTP Input │
│  Form      │
└──────┬─────┘
       │
       │ 2. POST /api/v1/auth/login-step-two
       │    { email, otp }
       ▼
┌─────────────────────────────────────────────────────────┐
│  AuthController.LoginStepTwo()                          │
└──────────────────────────┬──────────────────────────────┘
                           │
                           │ Send Command
                           ▼
┌─────────────────────────────────────────────────────────┐
│  LoginStepTwoCommandHandler                             │
├─────────────────────────────────────────────────────────┤
│  1. Get stored OTP from Redis                           │
│  2. Compare with provided OTP                           │
│  3. If invalid: Return Result.Failure()                 │
│  4. Get Account aggregate from database                 │
│  5. Generate JWT access token (15 min)                  │
│     • Claims: sub, email, role, AccountId, PartnerId    │
│  6. Generate refresh token (7 days)                     │
│  7. Store refresh token in database                     │
│  8. Delete OTP from Redis                               │
│  9. Return Result.Success(tokens)                       │
└──────────────────────────┬──────────────────────────────┘
                           │
                           │ Result<TokenResponse>
                           ▼
┌────────────┐
│   Client   │◄───── 200 OK {
│            │         accessToken: "eyJ...",
│            │         refreshToken: "guid...",
│            │         expiresAt: "2024-12-19T07:15:00Z"
│  Store     │       }
│  Tokens    │
│            │
│  Redirect  │
│  to        │
│  Dashboard │
└────────────┘
```

### Token Refresh Flow

```
┌────────────┐
│   Client   │
│            │
│  (Access   │
│   token    │
│  expired)  │
└──────┬─────┘
       │
       │ POST /api/v1/auth/refresh-token
       │ { refreshToken }
       ▼
┌─────────────────────────────────────────────────────────┐
│  RefreshTokenCommandHandler                             │
├─────────────────────────────────────────────────────────┤
│  1. Find refresh token in database                      │
│  2. Check if token is expired                           │
│  3. Check if token is revoked                           │
│  4. Get associated Account                              │
│  5. Generate new JWT access token (15 min)              │
│  6. Generate new refresh token (7 days)                 │
│  7. Revoke old refresh token                            │
│  8. Store new refresh token                             │
│  9. Return Result.Success(newTokens)                    │
└──────────────────────────┬──────────────────────────────┘
                           │
                           │ Result<TokenResponse>
                           ▼
┌────────────┐
│   Client   │◄───── 200 OK {
│            │         accessToken: "eyJ...",
│            │         refreshToken: "new-guid...",
│            │         expiresAt: "2024-12-19T07:30:00Z"
│  Update    │       }
│  Stored    │
│  Tokens    │
└────────────┘
```

### JWT Token Structure

```
┌─────────────────────────────────────────────────────────────────┐
│                         JWT Access Token                         │
├─────────────────────────────────────────────────────────────────┤
│  Header:                                                         │
│  {                                                               │
│    "alg": "HS256",                                               │
│    "typ": "JWT"                                                  │
│  }                                                               │
├─────────────────────────────────────────────────────────────────┤
│  Payload (Claims):                                               │
│  {                                                               │
│    "sub": "aspnet-user-id",              // ASP.NET Identity    │
│    "email": "user@example.com",                                 │
│    "role": "Partner",                     // User/Partner/Admin │
│    "AccountId": "guid",                   // TripEnjoy Account  │
│    "PartnerId": "guid",                   // If Partner role    │
│    "UserId": "guid",                      // If User role       │
│    "jti": "unique-jwt-id",                                      │
│    "iat": 1734594000,                     // Issued at          │
│    "exp": 1734594900,                     // Expires (15 min)   │
│    "iss": "TripEnjoy",                    // Issuer             │
│    "aud": "TripEnjoyAPI"                  // Audience           │
│  }                                                               │
├─────────────────────────────────────────────────────────────────┤
│  Signature:                                                      │
│  HMACSHA256(                                                     │
│    base64UrlEncode(header) + "." +                               │
│    base64UrlEncode(payload),                                     │
│    secret                                                        │
│  )                                                               │
└─────────────────────────────────────────────────────────────────┘
```

---

## 5. Domain Model

### Aggregate Map

```
┌─────────────────────────────────────────────────────────────────┐
│                     DOMAIN AGGREGATES                            │
└─────────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────────┐
│  ACCOUNT AGGREGATE                                   ✅ COMPLETE │
├────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─────────────────┐                                            │
│  │    Account      │◄──────────────────┐                        │
│  │  (Aggregate Root│                   │                        │
│  └────────┬────────┘                   │                        │
│           │                            │                        │
│           │ 1:1                   1:1  │                        │
│           ├──────────┬─────────────────┤                        │
│           │          │                 │                        │
│      ┌────▼────┐ ┌──▼───────┐  ┌──────▼──────┐                 │
│      │  User   │ │ Partner  │  │   Wallet    │                 │
│      │         │ └────┬─────┘  └─────────────┘                 │
│      └─────────┘      │                                         │
│                       │ 1:N                                     │
│                  ┌────▼──────────────┐                          │
│                  │ PartnerDocument   │                          │
│                  └───────────────────┘                          │
│           │                                                     │
│           │ 1:N                                                 │
│      ┌────▼────────────┐                                        │
│      │ RefreshToken    │                                        │
│      └─────────────────┘                                        │
│           │                                                     │
│           │ 1:N                                                 │
│      ┌────▼────────────┐                                        │
│      │ BlackListToken  │                                        │
│      └─────────────────┘                                        │
│                                                                 │
│  Value Objects:                                                 │
│  • AccountId, UserId, PartnerId, WalletId                       │
│  • RefreshTokenId, BlackListTokenId, PartnerDocumentId          │
│                                                                 │
│  Enums:                                                         │
│  • AccountStatusEnum, PartnerStatusEnum                         │
│  • PartnerDocumentType, PartnerDocumentStatusEnum               │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  PROPERTY AGGREGATE                                  ✅ ENHANCED  │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌─────────────────┐                                             │
│  │   Property      │                                             │
│  │ (Aggregate Root)│                                             │
│  └────────┬────────┘                                             │
│           │                                                      │
│           │ 1:N                                                  │
│      ┌────▼──────────────┐                                       │
│      │ PropertyImage     │                                       │
│      └───────────────────┘                                       │
│                                                                  │
│  References:                                                     │
│  • PropertyTypeId (FK to PropertyType aggregate)                 │
│  • PartnerId (FK to Account aggregate)                           │
│                                                                  │
│  Value Objects:                                                  │
│  • PropertyId, PropertyImageId                                   │
│                                                                  │
│  Missing Entities (Planned):                                     │
│  • RoomType, RoomTypeImage                                       │
│  • RoomAvailability, RoomPromotion                               │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  PROPERTYTYPE AGGREGATE                              ✅ COMPLETE  │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌─────────────────┐                                             │
│  │  PropertyType   │                                             │
│  │ (Aggregate Root)│                                             │
│  └─────────────────┘                                             │
│                                                                  │
│  Value Objects:                                                  │
│  • PropertyTypeId                                                │
│                                                                  │
│  Enum:                                                           │
│  • PropertyTypeEnum (Hotel, Apartment, Resort, Villa, ...)       │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  AUDITLOG AGGREGATE                                  ✅ COMPLETE  │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌─────────────────┐                                             │
│  │   AuditLog      │                                             │
│  │ (Aggregate Root)│                                             │
│  └─────────────────┘                                             │
│                                                                  │
│  Tracks:                                                         │
│  • Entity name                                                   │
│  • Action (Create, Update, Delete)                               │
│  • Old values / New values                                       │
│  • Timestamp, User                                               │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  MISSING AGGREGATES (Planned)                        ❌ TODO     │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  • Booking (Booking, BookingDetail, BookingHistory, Payment)     │
│  • Review (Review, ReviewImage, ReviewReply)                     │
│  • Voucher (Voucher, VoucherTarget, BookingVoucher)              │
│  • Financial (Transaction, Settlement)                           │
└─────────────────────────────────────────────────────────────────┘
```

### DDD Pattern Implementation

```
┌─────────────────────────────────────────────────────────────────┐
│                   VALUE OBJECT PATTERN                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  public class PropertyId : ValueObject                           │
│  {                                                               │
│      public Guid Id { get; private set; }                        │
│                                                                  │
│      private PropertyId(Guid id) => Id = id;                     │
│                                                                  │
│      // Factory Methods                                          │
│      public static PropertyId Create(Guid id) => new(id);        │
│      public static PropertyId CreateUnique() => new(Guid.New());│
│                                                                  │
│      // Equality                                                 │
│      public override IEnumerable<object> GetEqualityComponents() │
│      {                                                           │
│          yield return Id;                                        │
│      }                                                           │
│  }                                                               │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                   AGGREGATE ROOT PATTERN                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  public class Property : AggregateRoot<PropertyId>               │
│  {                                                               │
│      // Encapsulated state                                       │
│      public string Name { get; private set; }                    │
│      public PartnerId PartnerId { get; private set; }            │
│                                                                  │
│      // Collection encapsulation                                 │
│      private readonly List<PropertyImage> _images = new();       │
│      public IReadOnlyList<PropertyImage> Images =>               │
│          _images.AsReadOnly();                                   │
│                                                                  │
│      // EF Core constructor                                      │
│      private Property() : base(PropertyId.CreateUnique()) { }    │
│                                                                  │
│      // Factory method with validation                           │
│      public static Result<Property> Create(...)                  │
│      {                                                           │
│          // Business rule validation                             │
│          if (string.IsNullOrWhiteSpace(name))                    │
│              return Result<Property>.Failure(                    │
│                  DomainError.Property.InvalidName);              │
│                                                                  │
│          return Result<Property>.Success(new Property(...));     │
│      }                                                           │
│                                                                  │
│      // Business behavior                                        │
│      public Result AddImage(string imageUrl)                     │
│      {                                                           │
│          var image = PropertyImage.Create(this.Id, imageUrl);    │
│          _images.Add(image);                                     │
│          UpdatedAt = DateTime.UtcNow;                            │
│          return Result.Success();                                │
│      }                                                           │
│  }                                                               │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                      RESULT PATTERN                              │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  public class Result<T>                                          │
│  {                                                               │
│      public bool IsSuccess { get; }                              │
│      public bool IsFailure => !IsSuccess;                        │
│      public T Value { get; }                                     │
│      public List<Error> Errors { get; }                          │
│                                                                  │
│      public static Result<T> Success(T value) => new(value);     │
│      public static Result<T> Failure(Error error) =>             │
│          new(new List<Error> { error });                         │
│  }                                                               │
│                                                                  │
│  Usage:                                                          │
│  var result = Property.Create(name, address);                    │
│  if (result.IsFailure)                                           │
│      return result.Errors;                                       │
│                                                                  │
│  var property = result.Value;                                    │
└─────────────────────────────────────────────────────────────────┘
```

---

## 6. Database Schema

### Entity Relationship Diagram (Simplified)

```
┌──────────────────┐         ┌──────────────────┐
│   AspNetUsers    │◄───────│    Accounts      │
│  (Identity)      │  1:1    │                  │
└──────────────────┘         └────────┬─────────┘
                                      │
                          ┌───────────┴───────────┐
                     1:1  │                       │ 1:1
                  ┌───────▼────────┐     ┌───────▼────────┐
                  │     Users      │     │    Partners    │
                  └────────────────┘     └────────┬───────┘
                                                  │
                                                  │ 1:N
                                         ┌────────▼────────┐
                                         │PartnerDocuments │
                                         └─────────────────┘

┌──────────────────┐
│   Accounts       │
└────────┬─────────┘
         │ 1:1
         │
    ┌────▼────────┐
    │   Wallets   │
    └─────────────┘

┌──────────────────┐
│   Accounts       │
└────────┬─────────┘
         │ 1:N
         ├──────────┬──────────────┐
         │          │              │
    ┌────▼─────┐  ┌▼──────────┐  ┌▼──────────────┐
    │RefreshTokens│BlackListTokens│ (Future)      │
    └──────────┘  └───────────┘  └───────────────┘

┌──────────────────┐         ┌──────────────────┐
│ PropertyTypes    │         │    Partners      │
└────────┬─────────┘         └────────┬─────────┘
         │ 1:N                     1:N│
         │                            │
         └──────────┬─────────────────┘
                    │
               ┌────▼────────┐
               │ Properties  │
               └────┬────────┘
                    │ 1:N
               ┌────▼────────────┐
               │ PropertyImages  │
               └─────────────────┘

(Future Entities - Not Yet Implemented)

┌─────────────────┐         ┌──────────────────┐
│   Properties    │         │    Accounts      │
└────────┬────────┘         └────────┬─────────┘
         │ 1:N                   1:N │
         │                           │
         └──────────┬────────────────┘
                    │
               ┌────▼────────┐
               │  Bookings   │
               └────┬────────┘
                    │ 1:N
               ┌────▼───────────┐
               │ BookingDetails │
               └────────────────┘

┌─────────────────┐
│   Properties    │
└────────┬────────┘
         │ 1:N
    ┌────▼──────────┐
    │   RoomTypes   │
    └───┬───────────┘
        │ 1:N
        ├─────────────┬─────────────┐
        │             │             │
  ┌─────▼──────┐ ┌───▼───────┐ ┌───▼──────────┐
  │RoomTypeImages│RoomAvail. │RoomPromotions│
  └────────────┘ └───────────┘ └──────────────┘
```

### Key Database Tables

```
┌─────────────────────────────────────────────────────────────────┐
│  IMPLEMENTED TABLES                                              │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ACCOUNT MANAGEMENT:                                             │
│  • Accounts (AccountId PK, AspNetUserId FK, Email, Status)       │
│  • Users (UserId PK, AccountId FK, FullName, DateOfBirth)        │
│  • Partners (PartnerId PK, AccountId FK, CompanyName, ...)       │
│  • PartnerDocuments (DocId PK, PartnerId FK, Type, Status, URL)  │
│  • Wallets (WalletId PK, AccountId FK, Balance)                  │
│  • RefreshTokens (TokenId PK, AccountId FK, Token, Expires)      │
│  • BlackListTokens (TokenId PK, AccountId FK, Token)             │
│                                                                  │
│  PROPERTY MANAGEMENT:                                            │
│  • Properties (PropertyId PK, PartnerId FK, Name, Address, ...)  │
│  • PropertyImages (ImageId PK, PropertyId FK, ImageUrl, IsCover) │
│  • PropertyTypes (TypeId PK, TypeName, Status)                   │
│                                                                  │
│  AUDIT & IDENTITY:                                               │
│  • AuditLogs (LogId PK, EntityName, Action, OldValue, NewValue)  │
│  • AspNetUsers (from ASP.NET Identity)                           │
│  • AspNetRoles (from ASP.NET Identity)                           │
│  • AspNetUserRoles (from ASP.NET Identity)                       │
│  • (+ other Identity tables)                                     │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  PLANNED TABLES (Not Implemented)                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ROOM MANAGEMENT:                                                │
│  • RoomTypes                                                     │
│  • RoomAvailability                                              │
│  • RoomPromotion                                                 │
│  • RoomTypeImages                                                │
│                                                                  │
│  BOOKING SYSTEM:                                                 │
│  • Bookings                                                      │
│  • BookingDetails                                                │
│  • BookingHistory                                                │
│  • Payments                                                      │
│                                                                  │
│  REVIEW SYSTEM:                                                  │
│  • Reviews                                                       │
│  • ReviewImages                                                  │
│  • ReviewReplies                                                 │
│                                                                  │
│  VOUCHER SYSTEM:                                                 │
│  • Vouchers                                                      │
│  • VoucherTargets                                                │
│  • BookingVouchers                                               │
│                                                                  │
│  FINANCIAL:                                                      │
│  • Transactions                                                  │
│  • Settlements                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## 7. API Structure

### API Endpoint Map

```
TripEnjoy API (v1.0)
https://localhost:7199/api/v1/

├── /auth ───────────────────────────── [Rate Limited: 5/min]
│   ├── POST /register-user
│   ├── POST /register-partner
│   ├── POST /login-user-step-one
│   ├── POST /login-partner-step-one
│   ├── POST /login-step-two
│   ├── POST /refresh-token
│   ├── POST /logout ───────────────── [Auth: Required]
│   ├── POST /resend-otp
│   ├── GET  /confirm-email
│   ├── POST /forgot-password
│   └── POST /verify-reset-otp
│
├── /properties ─────────────────────── [Rate Limited: 100/min]
│   ├── GET  / ─────────────────────── [Auth: None] (Public)
│   ├── GET  /{id} ─────────────────── [Auth: None] (Public)
│   ├── GET  /my-properties ─────────── [Auth: Partner]
│   ├── POST / ─────────────────────── [Auth: Partner]
│   └── PUT  /{id} ─────────────────── [Auth: Partner + Owner]
│
├── /property-images ────────────────── [Rate Limited: 100/min]
│   ├── POST /generate-upload-url ───── [Auth: Partner]
│   ├── POST / ─────────────────────── [Auth: Partner]
│   ├── DELETE /{id} ───────────────── [Auth: Partner + Owner]
│   └── PUT  /{id}/set-cover ─────────── [Auth: Partner + Owner]
│
├── /property-types ─────────────────── [Rate Limited: 100/min]
│   └── GET  / ─────────────────────── [Auth: None] (Public)
│
└── /partner ────────────────────────── [Rate Limited: 100/min]
    └── GET  /documents ─────────────── [Auth: Partner]
```

### API Response Format

```
┌─────────────────────────────────────────────────────────────────┐
│  SUCCESS RESPONSE (200 OK)                                       │
├─────────────────────────────────────────────────────────────────┤
│  {                                                               │
│    "success": true,                                              │
│    "message": "Operation completed successfully",                │
│    "data": {                                                     │
│      // Response payload                                         │
│      "propertyId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",       │
│      "name": "Luxury Villa Resort"                               │
│    },                                                            │
│    "errors": []                                                  │
│  }                                                               │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  ERROR RESPONSE (4xx / 5xx)                                      │
├─────────────────────────────────────────────────────────────────┤
│  {                                                               │
│    "success": false,                                             │
│    "message": "Operation failed",                                │
│    "data": null,                                                 │
│    "errors": [                                                   │
│      {                                                           │
│        "code": "Property.NotFound",                              │
│        "message": "The property was not found.",                 │
│        "type": "NotFound"                                        │
│      }                                                           │
│    ]                                                             │
│  }                                                               │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  VALIDATION ERROR RESPONSE (400 Bad Request)                     │
├─────────────────────────────────────────────────────────────────┤
│  {                                                               │
│    "success": false,                                             │
│    "message": "Validation failed",                               │
│    "data": null,                                                 │
│    "errors": [                                                   │
│      {                                                           │
│        "code": "Validation.PropertyName",                        │
│        "message": "Property name must be between 3 and 100 ...", │
│        "type": "Validation"                                      │
│      },                                                          │
│      {                                                           │
│        "code": "Validation.PropertyAddress",                     │
│        "message": "Address is required.",                        │
│        "type": "Validation"                                      │
│      }                                                           │
│    ]                                                             │
│  }                                                               │
└─────────────────────────────────────────────────────────────────┘
```

---

## 8. Deployment Architecture

### Production Environment

```
┌─────────────────────────────────────────────────────────────────┐
│                      LOAD BALANCER                               │
│                   (Azure/AWS/On-Prem)                            │
└─────────────────────────┬───────────────────────────────────────┘
                          │
          ┌───────────────┼───────────────┐
          │               │               │
  ┌───────▼──────┐ ┌─────▼──────┐ ┌─────▼──────┐
  │   Web Server │ │  Web Server│ │  Web Server│
  │   Instance 1 │ │  Instance 2│ │  Instance 3│
  └───────┬──────┘ └─────┬──────┘ └─────┬──────┘
          │              │              │
          │   TripEnjoy.Api (Kestrel)   │
          │   TripEnjoy.Client (MVC)    │
          │                             │
          └──────────────┬──────────────┘
                         │
        ┌────────────────┼────────────────┐
        │                │                │
  ┌─────▼──────┐  ┌─────▼──────┐  ┌─────▼──────┐
  │  SQL Server│  │   Redis    │  │ Cloudinary │
  │  (Primary) │  │   Cache    │  │   (CDN)    │
  └─────┬──────┘  └────────────┘  └────────────┘
        │
  ┌─────▼──────┐
  │  SQL Server│
  │ (Replica)  │
  └────────────┘

  Background Services:
  ┌──────────────────┐
  │  Hangfire Worker │
  │  • Job Processing│
  │  • Email Queue   │
  │  • Cleanup Tasks │
  └──────────────────┘
```

### Technology Stack Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    TECHNOLOGY STACK                              │
└─────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│  FRONTEND                                                         │
├──────────────────────────────────────────────────────────────────┤
│  • ASP.NET Core MVC (Razor)                                       │
│  • Bootstrap 5                                                    │
│  • jQuery                                                         │
│  • SweetAlert2                                                    │
└──────────────────────────────────────────────────────────────────┘
                               │
                               ▼
┌──────────────────────────────────────────────────────────────────┐
│  BACKEND API                                                      │
├──────────────────────────────────────────────────────────────────┤
│  • .NET 8                                                         │
│  • ASP.NET Core Web API                                           │
│  • JWT Bearer Authentication                                      │
│  • Swagger / Swashbuckle                                          │
│  • Rate Limiting                                                  │
└──────────────────────────────────────────────────────────────────┘
                               │
                               ▼
┌──────────────────────────────────────────────────────────────────┐
│  APPLICATION LAYER                                                │
├──────────────────────────────────────────────────────────────────┤
│  • MediatR (CQRS)                                                 │
│  • FluentValidation                                               │
│  • AutoMapper (if used)                                           │
└──────────────────────────────────────────────────────────────────┘
                               │
                               ▼
┌──────────────────────────────────────────────────────────────────┐
│  INFRASTRUCTURE                                                   │
├──────────────────────────────────────────────────────────────────┤
│  • Entity Framework Core 8                                        │
│  • SQL Server                                                     │
│  • Redis (StackExchange.Redis)                                    │
│  • Hangfire (Background Jobs)                                     │
│  • Serilog (Logging)                                              │
│  • Cloudinary (Image Storage)                                     │
│  • SMTP (Email)                                                   │
└──────────────────────────────────────────────────────────────────┘
                               │
                               ▼
┌──────────────────────────────────────────────────────────────────┐
│  TESTING                                                          │
├──────────────────────────────────────────────────────────────────┤
│  • xUnit                                                          │
│  • Moq                                                            │
│  • FluentAssertions                                               │
│  • AutoFixture                                                    │
│  • Bogus                                                          │
│  • WebApplicationFactory                                          │
└──────────────────────────────────────────────────────────────────┘
```

---

## Summary

This document provides comprehensive visual representations of the TripEnjoy platform architecture. Key takeaways:

- **Clean Architecture**: Clear separation of concerns across 8 projects
- **DDD Implementation**: Strong domain models with proper aggregate boundaries
- **CQRS Pattern**: Scalable request handling with MediatR pipeline
- **Security-First**: Two-factor authentication with JWT tokens
- **Modern Stack**: .NET 8 with industry-standard libraries
- **Test Coverage**: Comprehensive unit and integration testing
- **Production-Ready**: Scalable deployment architecture

For detailed implementation guidelines, refer to:
- `docs/PROJECT-ANALYSIS.md` - Complete project analysis
- `docs/TripEnjoy-Project-Context.md` - Business context
- `docs/DDD-Domain-Constraints.md` - DDD implementation rules

---

**Last Updated**: December 19, 2024
