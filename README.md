# TripEnjoy - Room Booking Platform

TripEnjoy is an enterprise-grade room booking platform built with .NET 8 that connects travelers with accommodation partners. The platform implements Clean Architecture with Domain-Driven Design (DDD) principles.

## 🌟 Quick Stats

- **Architecture**: Clean Architecture + DDD + CQRS
- **Platform**: .NET 8
- **Projects**: 8 (Domain, Application, Infrastructure, API, Client, Tests)
- **Lines of Code**: ~13,845
- **Test Coverage**: 139 test cases (114 unit + 25 integration)
- **API Endpoints**: 20+
- **Production Ready**: 60%

## 🛠️ Tech Stack

### Backend
- **.NET 8** - Core platform
- **Entity Framework Core 8** - ORM
- **SQL Server** - Database
- **MediatR 11** - CQRS implementation
- **FluentValidation 12** - Input validation
- **JWT Bearer** - Authentication
- **Redis** - Distributed caching
- **Hangfire** - Background jobs
- **Serilog** - Structured logging
- **Cloudinary** - Image storage

### Frontend
- **ASP.NET Core MVC** - Server-side rendering
- **Razor** - View engine
- **Bootstrap 5** - CSS framework
- **jQuery** - JavaScript library

### Testing
- **xUnit 2.5** - Test framework
- **Moq 4.20** - Mocking framework
- **FluentAssertions 6.12** - Assertion library
- **AutoFixture** - Test data generation

## 🏗️ Architecture

### Clean Architecture Layers

```
Presentation (API + Client)
       ↓
Application (CQRS/MediatR)
       ↓
Domain (Entities + Business Logic)
       ↑
Infrastructure (Repositories + External Services)
```

### Key Patterns

- **Domain-Driven Design (DDD)**: Rich domain models with aggregates and value objects
- **CQRS**: Command Query Responsibility Segregation using MediatR
- **Repository Pattern**: Data access abstraction
- **Unit of Work**: Transaction management
- **Result Pattern**: Type-safe error handling without exceptions

## 🔐 Features

### ✅ Implemented

- **Two-Factor Authentication**: OTP-based login with JWT tokens
- **User Management**: User and Partner registration with role-based access
- **Partner Onboarding**: Complete workflow with document verification
- **Property Management**: Full CRUD operations with image management
- **Secure Image Upload**: Cloudinary integration with signed URLs
- **Digital Wallet**: Account balance management
- **Audit Logging**: Entity change tracking

### 🚧 In Progress

- **Room Management**: Room types, availability, pricing
- **Booking System**: Reservation workflow and payment processing
- **Review System**: Guest feedback and ratings
- **Advanced Search**: Property discovery with filters

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server
- Redis Server
- Cloudinary Account (for image storage)

### Installation

1. Clone the repository
```bash
git clone https://github.com/Hao-Nguyen2712/TripEnjoy-Solution.git
cd TripEnjoy-Solution
```

2. Update connection strings in `appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your-sql-server-connection"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

3. Run migrations
```bash
cd src/TripEnjoyServer/TripEnjoy.Api
dotnet ef database update
```

4. Run the API
```bash
dotnet run --project src/TripEnjoyServer/TripEnjoy.Api
```

5. Run the MVC Client (separate terminal)
```bash
dotnet run --project src/TripEnjoyServer/TripEnjoy.Client
```

### Access Points

- **API**: https://localhost:7199
- **Swagger**: https://localhost:7199/swagger
- **Hangfire Dashboard**: https://localhost:7199/hangfire
- **MVC Client**: https://localhost:7100 (check launchSettings.json)

## 📚 Documentation

Comprehensive documentation is available in the `docs/` folder:

- **[EXECUTIVE-SUMMARY.md](docs/EXECUTIVE-SUMMARY.md)** - Quick overview and key highlights
- **[PROJECT-ANALYSIS.md](docs/PROJECT-ANALYSIS.md)** - Complete technical analysis (42KB)
- **[ARCHITECTURE-DIAGRAMS.md](docs/ARCHITECTURE-DIAGRAMS.md)** - Visual architecture diagrams (55KB)
- **[TripEnjoy-Project-Context.md](docs/TripEnjoy-Project-Context.md)** - Business context and domain
- **[DDD-Domain-Constraints.md](docs/DDD-Domain-Constraints.md)** - DDD implementation guidelines

## 🧪 Testing

The project includes comprehensive unit and integration tests with proper categorization.

### Run All Tests
```bash
dotnet test
```

### Run Unit Tests Only
```bash
dotnet test --filter "Category!=Integration"
```

### Run Integration Tests Only
```bash
dotnet test --filter "Category=Integration"
```

### Run Specific Test Project
```bash
dotnet test src/TripEnjoyServer/TripEnjoy.Test
```

### Test Results
- **Unit Tests**: 114 tests ✅ All passing
- **Integration Tests**: 25 tests (require Redis and SQL Server)
- **Test Framework**: xUnit with FluentAssertions and Moq

## 📈 Project Status

| Component | Status | Completion |
|-----------|--------|-----------|
| Authentication | ✅ Complete | 100% |
| Partner Onboarding | ✅ Complete | 100% |
| Property Management | ✅ Complete | 100% |
| Room Management | 🚧 In Progress | 30% |
| Booking System | ❌ Todo | 0% |
| Review System | ❌ Todo | 0% |
| **Overall** | ⚠️ Partial | **60%** |

## 🗺️ Roadmap

### High Priority
- [ ] Room Management System
- [ ] Booking Engine
- [ ] Payment Integration

### Medium Priority
- [ ] Review & Rating System
- [ ] Advanced Search
- [ ] Admin Dashboard

### Low Priority
- [ ] Voucher System
- [ ] Mobile Application
- [ ] Analytics Dashboard

## 👥 Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Follow TDD principles (write tests first)
4. Follow existing architectural patterns
5. Update documentation as needed
6. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
7. Push to the branch (`git push origin feature/AmazingFeature`)
8. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- Clean Architecture principles by Robert C. Martin
- Domain-Driven Design by Eric Evans
- CQRS pattern by Greg Young
- .NET community for excellent libraries and tools

## 📧 Contact

**Hao Nguyen** - [@Hao-Nguyen2712](https://github.com/Hao-Nguyen2712)

**Project Link**: [https://github.com/Hao-Nguyen2712/TripEnjoy-Solution](https://github.com/Hao-Nguyen2712/TripEnjoy-Solution)

---

⭐ **Star this repository** if you find it helpful!


