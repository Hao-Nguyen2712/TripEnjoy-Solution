# Admin Dashboard and Management Features

## Overview

The TripEnjoy Admin Dashboard provides comprehensive platform management capabilities for administrators. This document outlines the available features, API endpoints, and usage guidelines.

## Features

### 1. Admin Dashboard (`/admin/dashboard`)

**Purpose**: Provides a comprehensive overview of platform statistics and quick access to management functions.

**Key Metrics**:
- Total Users, Partners, Properties, Bookings
- Revenue Statistics (Today, This Week, This Month, This Year)
- Pending Approvals (Partners and Properties)
- Active Vouchers and Reviews

**Quick Actions**:
- Navigate to all admin management pages
- View real-time platform health

**API Endpoint**: `GET /api/v1/admin/dashboard/stats`

---

### 2. User Management (`/admin/users`)

**Purpose**: Manage all platform users with block/unblock capabilities.

**Features**:
- List all registered users
- View user details (email, name, role, status, registration date)
- Block users (prevent platform access)
- Unblock users (restore platform access)

**API Endpoints**:
- `GET /api/v1/admin/users` - List all users
- `POST /api/v1/admin/users/{userId}/block` - Block a user
- `POST /api/v1/admin/users/{userId}/unblock` - Unblock a user

---

### 3. Partner Approvals (`/admin/partners`)

**Purpose**: Review and approve/reject partner applications.

**Features**:
- List pending partner applications
- View business name, email, status, requested date
- Review submitted documents
- Approve partner applications
- Reject partner applications with reason

**API Endpoints**:
- `GET /api/v1/admin/partners/pending` - List pending partner approvals
- `POST /api/v1/admin/partners/{partnerId}/approve` - Approve partner
- `POST /api/v1/admin/partners/{partnerId}/reject` - Reject partner

**Workflow**:
1. Partner submits application with business details and documents
2. Admin reviews application in Partner Approvals page
3. Admin approves or rejects with optional reason
4. Partner is notified of decision

---

### 4. Property Approvals (`/admin/properties`)

**Purpose**: Review and approve/reject property listings before they go live.

**Features**:
- List pending property listings
- View property name, address, status, submission date
- Approve property listings
- Reject property listings with reason

**API Endpoints**:
- `GET /api/v1/admin/properties/pending` - List pending property approvals
- `POST /api/v1/admin/properties/{propertyId}/approve` - Approve property
- `POST /api/v1/admin/properties/{propertyId}/reject` - Reject property

**Workflow**:
1. Partner creates property listing
2. Property enters "Inactive" status awaiting approval
3. Admin reviews property details
4. Admin approves (property becomes "Active") or rejects
5. Partner is notified of decision

---

### 5. Property Types Management (`/admin/property-types`)

**Purpose**: View available property type categories.

**Features**:
- List all property types (Hotel, Apartment, Villa, Resort, etc.)
- View property type details

**API Endpoint**: Uses existing property type endpoints

---

### 6. Bookings Management (`/admin/bookings`)

**Purpose**: Monitor and manage all platform bookings.

**Features**:
- List all bookings across the platform
- Filter by status (Pending, Confirmed, Cancelled, Completed)
- Search bookings
- View booking details (guest, property, dates, amount)
- Monitor booking status

**API Endpoint**: `GET /api/v1/admin/bookings` - List all bookings

**Booking Statuses**:
- **Pending**: Awaiting confirmation
- **Confirmed**: Booking confirmed
- **Cancelled**: Booking cancelled
- **Completed**: Booking completed

---

### 7. Vouchers Management (`/admin/vouchers`)

**Purpose**: Monitor and manage platform vouchers and discount codes.

**Features**:
- List all vouchers
- Filter by status (Active, Expired, Used)
- Search vouchers
- View voucher details (code, discount, usage, expiry date)
- Monitor voucher usage statistics

**API Endpoint**: `GET /api/v1/admin/vouchers` - List all vouchers

**Voucher Statuses**:
- **Active**: Currently valid and available
- **Expired**: Past expiry date
- **Used**: Fully redeemed (max usage reached)

---

### 8. Reviews Management (`/admin/reviews`)

**Purpose**: Placeholder for future review moderation features.

**Planned Features**:
- List all reviews
- Moderate inappropriate reviews
- Delete spam reviews
- View review statistics

---

## Authorization

All admin features require the `Admin` role. The endpoints are secured with:

```csharp
[Authorize(Roles = RoleConstant.Admin)]
```

**Access Control**:
- Users must be authenticated
- Users must have `Admin` role assigned
- Unauthorized access returns 401/403 responses

---

## API Response Format

All admin endpoints follow the Result pattern:

**Success Response**:
```json
{
  "isSuccess": true,
  "data": { /* response data */ },
  "message": "Operation successful"
}
```

**Error Response**:
```json
{
  "isSuccess": false,
  "error": {
    "code": "Error.Code",
    "message": "Error description"
  }
}
```

---

## Domain Changes

### Account Entity

Added flexible state transition methods for admin operations:
- `MarkAsBanned()` - Block user account
- `MarkAsLocked()` - Lock user account
- `MarkAsActive()` - Restore user account

### Partner Entity

Added approval workflow methods:
```csharp
public Result Approve()
{
    if (Status == PartnerStatusEnum.Approved.ToString())
        return Result.Failure(/* AlreadyApproved error */);
    
    Status = PartnerStatusEnum.Approved.ToString();
    return Result.Success();
}

public Result Reject(string reason)
{
    if (Status == PartnerStatusEnum.Rejected.ToString())
        return Result.Failure(/* AlreadyRejected error */);
    
    Status = PartnerStatusEnum.Rejected.ToString();
    RejectionReason = reason;
    return Result.Success();
}
```

### Property Entity

Added approval workflow methods:
```csharp
public Result Approve()
{
    if (Status == PropertyEnum.Active.ToString())
        return Result.Failure(/* AlreadyActive error */);
    
    Status = PropertyEnum.Active.ToString();
    return Result.Success();
}

public Result Reject(string reason)
{
    if (Status == PropertyEnum.Inactive.ToString())
        return Result.Failure(/* AlreadyInactive error */);
    
    Status = PropertyEnum.Inactive.ToString();
    RejectionReason = reason;
    return Result.Success();
}
```

---

## Frontend Architecture

### Admin Service (`IAdminService`)

Central service for all admin API communication:

```csharp
public interface IAdminService
{
    Task<Result<AdminDashboardStatsDto>> GetDashboardStatsAsync();
    Task<Result<List<UserManagementDto>>> GetAllUsersAsync();
    Task<Result> BlockUserAsync(Guid userId, string reason);
    Task<Result> UnblockUserAsync(Guid userId);
    Task<Result<List<PartnerApprovalDto>>> GetPendingPartnerApprovalsAsync();
    Task<Result> ApprovePartnerAsync(Guid partnerId);
    Task<Result> RejectPartnerAsync(Guid partnerId, string reason);
    Task<Result<List<PropertyApprovalDto>>> GetPendingPropertyApprovalsAsync();
    Task<Result> ApprovePropertyAsync(Guid propertyId);
    Task<Result> RejectPropertyAsync(Guid propertyId, string reason);
    Task<Result<List<BookingDetailDto>>> GetAllBookingsAsync();
    Task<Result<List<VoucherDto>>> GetAllVouchersAsync();
}
```

### Component Structure

Each admin page follows this structure:
1. **Authorization** - `<AuthorizeView Roles="Admin">`
2. **Loading State** - Progress indicator during data fetch
3. **Data Display** - MudBlazor table with data
4. **Actions** - Approve/Reject/Block/Unblock buttons
5. **Error Handling** - Snackbar notifications

---

## CQRS Implementation

### Commands

- `BlockUserCommand` - Block a user
- `UnblockUserCommand` - Unblock a user
- `ApprovePartnerCommand` - Approve partner application
- `RejectPartnerCommand` - Reject partner application
- `ApprovePropertyCommand` - Approve property listing
- `RejectPropertyCommand` - Reject property listing

### Queries

- `GetAdminDashboardStatsQuery` - Get dashboard statistics
- `GetAllUsersQuery` - Get all users
- `GetPendingPartnerApprovalsQuery` - Get pending partners
- `GetPendingPropertyApprovalsQuery` - Get pending properties
- `GetAllBookingsQuery` - Get all bookings
- `GetAllVouchersQuery` - Get all vouchers

### Handlers

All handlers follow the pattern:
1. Validate command/query
2. Retrieve data from repository via UnitOfWork
3. Apply domain logic
4. Save changes if mutation
5. Return Result

---

## Testing

### Manual Testing via Swagger

1. Navigate to `https://localhost:7199/swagger`
2. Authenticate as admin user
3. Test admin endpoints under `/api/v1/admin`

### Test Admin Account

Use the seeded admin account (if available):
- Email: `admin@tripenjoy.com`
- Password: (check DataSeeder)

---

## Security Considerations

### Authentication & Authorization
- JWT-based authentication required
- Role-based authorization (Admin role)
- Rate limiting on sensitive endpoints

### Input Validation
- FluentValidation for command validation
- Domain entity validation
- XSS prevention (Blazor auto-encodes)

### Audit Trail
- All admin actions should be logged (future enhancement)
- Track who approved/rejected what and when

---

## Future Enhancements

1. **Audit Logs** - Track all admin actions
2. **Financial Reports** - Revenue, commissions, settlements
3. **Review Moderation** - Full review management
4. **Advanced Filters** - Date ranges, complex queries
5. **Bulk Operations** - Approve/reject multiple items
6. **Email Notifications** - Notify users of admin actions
7. **Export Functionality** - Export data to CSV/Excel
8. **Analytics Dashboard** - Charts and graphs
9. **Role Management** - Create/manage admin roles
10. **System Settings** - Platform configuration

---

## Troubleshooting

### Cannot Access Admin Pages
- Verify user has `Admin` role
- Check JWT token is valid
- Ensure authentication state is properly set

### API Returns 403 Forbidden
- User lacks `Admin` role
- Token expired or invalid

### Pages Not Loading Data
- Check browser console for errors
- Verify API endpoints are accessible
- Check network tab for failed requests

---

## Related Documentation

- [Clean Architecture Overview](../TripEnjoy-Project-Context.md)
- [CQRS Pattern](../TripEnjoy-Project-Context.md)
- [Authentication Guide](../TripEnjoy-Project-Context.md)
- [API Documentation](https://localhost:7199/swagger)

---

**Version**: 1.0  
**Last Updated**: 2025-12-26  
**Author**: TripEnjoy Development Team
