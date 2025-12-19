# TripEnjoy Database Entity-Relationship Diagram (ERD)

## Overview
This document provides a comprehensive overview of the TripEnjoy database schema, including all tables, relationships, and business rules as defined in the Entity-Relationship Diagram.

---

## Table of Contents

1. [Identity & Authentication](#identity--authentication)
2. [User Management](#user-management)
3. [Property Management](#property-management)
4. [Room Management](#room-management)
5. [Booking Management](#booking-management)
6. [Payment & Financial](#payment--financial)
7. [Voucher & Promotions](#voucher--promotions)
8. [Review System](#review-system)
9. [System Auditing](#system-auditing)
10. [Relationship Summary](#relationship-summary)

---

## Identity & Authentication

### Account
**Table**: `Accounts`  
**Purpose**: Core user account management for all user types (User, Partner, Admin)

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| AccountId | UNIQUEIDENTIFIER | PK | Unique account identifier |
| AspNetUserId | UNIQUEIDENTIFIER | NOT NULL | Foreign key to ASP.NET Identity |
| AccountEmail | NVARCHAR | | User's email address |
| AccountIsDeleted | BIT | | Soft delete flag |
| AccountCreateAt | DATETIME | | Account creation timestamp |
| AccountUpdateAt | DATETIME | NULL | Last update timestamp |

**Relationships**:
- 1:1 with `User`
- 1:1 with `Partner`
- 1:1 with `Wallet`
- 1:N with `RefreshToken`
- 1:N with `BlacklistedToken`
- 1:N with `AuditLog`

---

### RefreshToken
**Table**: `RefreshTokens`  
**Purpose**: JWT refresh token management for maintaining user sessions

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| RefreshTokenId | UNIQUEIDENTIFIER | PK | Unique token identifier |
| AccountId | UNIQUEIDENTIFIER | FK | Reference to Account |
| Token | NVARCHAR(500) | NOT NULL | Refresh token value |
| ExpiryDate | DATETIME | NOT NULL | Token expiration date |
| CreatedAt | DATETIME | NOT NULL | Token creation timestamp |
| RevokedAt | DATETIME | NULL | Revocation timestamp (null = active) |
| IsUsed | BIT | NOT NULL | Single-use flag |

**Business Rules**:
- Each refresh token can only be used once (IsUsed flag)
- Tokens expire after 7 days (default)
- Revoked tokens cannot be reused (RevokedAt is set)

**Relationships**:
- N:1 with `Account` (AccountId)

---

### BlacklistedToken
**Table**: `BlacklistedTokens`  
**Purpose**: Blacklist for invalidated access tokens (logout, security)

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| BlacklistedTokenId | UNIQUEIDENTIFIER | PK | Unique blacklist entry identifier |
| AccountId | UNIQUEIDENTIFIER | FK | Reference to Account |
| Token | NVARCHAR(500) | NOT NULL | Blacklisted access token |
| ExpiredAt | DATETIME | NOT NULL | Token expiration time |
| CreatedAt | DATETIME | NOT NULL | Blacklist entry timestamp |

**Business Rules**:
- Blacklisted tokens cannot be used for authentication
- Tokens are automatically removed after ExpiredAt for cleanup
- Used for logout and security incidents

**Relationships**:
- N:1 with `Account` (AccountId)

---

## User Management

### User
**Table**: `Users`  
**Purpose**: End-user (guest/traveler) profile information

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| UserId | UNIQUEIDENTIFIER | PK | Unique user identifier |
| AccountId | UNIQUEIDENTIFIER | FK | Reference to Account |
| Fullname | NVARCHAR | | User's full name |
| PhoneNumber | NVARCHAR | | Contact phone number |
| Address | NVARCHAR | | Physical address |
| DateOfBirth | DATE | | User's birth date |
| Gender | NVARCHAR | | User's gender |

**Relationships**:
- 1:1 with `Account` (AccountId)
- 1:N with `Booking` (as guest)
- 1:N with `Review` (as reviewer)

---

### Partner
**Table**: `Partners`  
**Purpose**: Property owner/manager profile information

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| PartnerId | UNIQUEIDENTIFIER | PK | Unique partner identifier |
| AccountId | UNIQUEIDENTIFIER | FK | Reference to Account |
| CompanyName | NVARCHAR | | Business/company name |
| Status | NVARCHAR | | Partner status (Pending, Approved, Rejected, Suspended) |

**Business Rules**:
- Partners must be approved before listing properties
- Status workflow: Pending → Approved/Rejected
- Suspended partners cannot receive new bookings

**Relationships**:
- 1:1 with `Account` (AccountId)
- 1:N with `PartnerDocument` (verification documents)
- 1:N with `Property` (owned properties)
- 1:N with `ReviewReply` (as replier)
- 1:N with `VoucherTarget` (partner-specific vouchers)

---

### PartnerDocument
**Table**: `PartnerDocuments`  
**Purpose**: Partner verification documents for approval process

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| DocumentId | UNIQUEIDENTIFIER | PK | Unique document identifier |
| PartnerId | UNIQUEIDENTIFIER | FK | Reference to Partner |
| DocumentType | NVARCHAR(100) | NOT NULL | Document category |
| FilePath | NVARCHAR(500) | NOT NULL | Cloud storage path |
| Status | NVARCHAR(50) | NOT NULL | Review status |
| UploadedAt | DATETIME | NOT NULL | Upload timestamp |
| ReviewedAt | DATETIME | NULL | Review completion timestamp |

**Document Types**:
- Business License
- Tax Identification
- Proof of Address
- Identity Verification
- Insurance Certificate

**Relationships**:
- N:1 with `Partner` (PartnerId)

---

## Property Management

### Property
**Table**: `Properties`  
**Purpose**: Accommodation property listings

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| PropertyId | UNIQUEIDENTIFIER | PK | Unique property identifier |
| PartnerId | UNIQUEIDENTIFIER | FK | Reference to Partner (owner) |
| PropertyTypeId | UNIQUEIDENTIFIER | FK | Reference to PropertyType |
| Name | NVARCHAR(200) | NOT NULL | Property name |
| Description | NVARCHAR(MAX) | | Detailed description |
| Address | NVARCHAR(500) | NOT NULL | Street address |
| City | NVARCHAR(100) | NOT NULL | City location |
| Country | NVARCHAR(100) | NOT NULL | Country location |
| Latitude | DECIMAL(9,6) | | GPS latitude |
| Longitude | DECIMAL(9,6) | | GPS longitude |
| Status | NVARCHAR(50) | NOT NULL | Property status |
| AverageRating | DECIMAL(3,2) | NULL | Calculated average rating |
| ReviewCount | INT | DEFAULT 0 | Number of reviews |
| CreatedAt | DATETIME | NOT NULL | Creation timestamp |
| UpdatedAt | DATETIME | NULL | Last update timestamp |

**Business Rules**:
- Properties must be approved by admin before appearing in search
- Average rating is calculated from room reviews
- Partners can only manage their own properties

**Relationships**:
- N:1 with `Partner` (PartnerId)
- N:1 with `PropertyType` (PropertyTypeId)
- 1:N with `PropertyImage` (property photos)
- 1:N with `RoomType` (room inventory)
- 1:N with `VoucherTarget` (property-specific vouchers)

---

### PropertyType
**Table**: `PropertyTypes`  
**Purpose**: Property categorization (Hotel, Apartment, Resort, etc.)

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| PropertyTypeId | UNIQUEIDENTIFIER | PK | Unique type identifier |
| Name | NVARCHAR | NOT NULL | Type name |
| Status | NVARCHAR | NOT NULL | Active/Inactive status |

**Property Type Values**:
- Hotel
- Apartment
- Resort
- Villa
- Cabin
- Guest House
- Hostel
- Motel

**Relationships**:
- 1:N with `Property` (PropertyTypeId)

---

### PropertyImage
**Table**: `PropertyImages`  
**Purpose**: Property photo gallery

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| ImageId | UNIQUEIDENTIFIER | PK | Unique image identifier |
| PropertyId | UNIQUEIDENTIFIER | FK | Reference to Property |
| FilePath | NVARCHAR(500) | NOT NULL | Cloud storage path |
| IsMain | BIT | NOT NULL | Cover photo flag |
| UploadedAt | DATETIME | NOT NULL | Upload timestamp |

**Business Rules**:
- Each property should have at least one image marked as IsMain
- Images are stored in Cloudinary
- Maximum recommended: 20 images per property

**Relationships**:
- N:1 with `Property` (PropertyId)

---

## Room Management

### RoomType
**Table**: `RoomTypes`  
**Purpose**: Room type/category within a property (e.g., Deluxe Room, Standard Suite)

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| RoomTypeId | UNIQUEIDENTIFIER | PK | Unique room type identifier |
| PropertyId | UNIQUEIDENTIFIER | FK | Reference to Property |
| RoomTypeName | NVARCHAR(200) | NOT NULL | Room type name |
| Description | NVARCHAR(MAX) | | Detailed description |
| Capacity | INT | NOT NULL | Maximum guest capacity |
| BasePrice | DECIMAL(18,2) | NOT NULL | Base price per night |
| TotalQuantity | INT | NOT NULL | Total rooms of this type |
| Status | NVARCHAR(50) | NOT NULL | Active/Inactive |
| AverageRating | DECIMAL(3,2) | NULL | Calculated average rating |
| ReviewCount | INT | DEFAULT 0 | Number of reviews |
| CreatedAt | DATETIME | NOT NULL | Creation timestamp |
| UpdatedAt | DATETIME | NULL | Last update timestamp |

**Business Rules**:
- Room types belong to a specific property
- BasePrice is the default, can be overridden by RoomAvailability
- TotalQuantity represents the total inventory for this room type

**Relationships**:
- N:1 with `Property` (PropertyId)
- 1:N with `RoomTypeImage` (room photos)
- 1:N with `RoomAvailability` (daily availability)
- 1:N with `RoomPromotion` (discounts)
- 1:N with `BookingDetail` (booked rooms)
- 1:N with `Review` (room reviews)
- 1:N with `VoucherTarget` (room-specific vouchers)

---

### RoomTypeImage
**Table**: `RoomTypeImages`  
**Purpose**: Room type photo gallery

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| ImageId | UNIQUEIDENTIFIER | PK | Unique image identifier |
| RoomTypeId | UNIQUEIDENTIFIER | FK | Reference to RoomType |
| FilePath | NVARCHAR(500) | NOT NULL | Cloud storage path |
| IsMain | BIT | NOT NULL | Cover photo flag |
| UploadedAt | DATETIME | NOT NULL | Upload timestamp |

**Relationships**:
- N:1 with `RoomType` (RoomTypeId)

---

### RoomAvailability
**Table**: `RoomAvailabilities`  
**Purpose**: Daily room availability and dynamic pricing

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| AvailabilityId | UNIQUEIDENTIFIER | PK | Unique availability record identifier |
| RoomTypeId | UNIQUEIDENTIFIER | FK | Reference to RoomType |
| Date | DATE | NOT NULL | Specific date |
| AvailableQuantity | INT | NOT NULL | Rooms available for booking |
| Price | DECIMAL(18,2) | NULL | Override price (null = use BasePrice) |
| CreatedAt | DATETIME | NOT NULL | Record creation timestamp |
| UpdatedAt | DATETIME | NULL | Last update timestamp |

**Business Rules**:
- One record per RoomType per Date
- AvailableQuantity decreases when bookings are made
- If Price is NULL, use RoomType.BasePrice
- AvailableQuantity cannot be negative

**Relationships**:
- N:1 with `RoomType` (RoomTypeId)

---

### RoomPromotion
**Table**: `RoomPromotions`  
**Purpose**: Promotional discounts for room types

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| PromotionId | UNIQUEIDENTIFIER | PK | Unique promotion identifier |
| RoomTypeId | UNIQUEIDENTIFIER | FK | Reference to RoomType |
| DiscountPercent | DECIMAL(5,2) | NULL | Percentage discount (0-100) |
| DiscountAmount | DECIMAL(18,2) | NULL | Fixed amount discount |
| StartDate | DATETIME | NOT NULL | Promotion start date |
| EndDate | DATETIME | NOT NULL | Promotion end date |
| Status | NVARCHAR(50) | NOT NULL | Active/Expired/Disabled |
| CreatedAt | DATETIME | NOT NULL | Creation timestamp |

**Business Rules**:
- Either DiscountPercent OR DiscountAmount must be set (not both)
- Applied automatically during booking if active
- Cannot overlap with voucher discounts (choose best for customer)

**Relationships**:
- N:1 with `RoomType` (RoomTypeId)

---

## Booking Management

### Booking
**Table**: `Bookings`  
**Purpose**: Customer reservation records

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| BookingId | UNIQUEIDENTIFIER | PK | Unique booking identifier |
| UserId | UNIQUEIDENTIFIER | FK | Reference to User (guest) |
| CheckInDate | DATE | NOT NULL | Check-in date |
| CheckOutDate | DATE | NOT NULL | Check-out date |
| TotalAmount | DECIMAL(18,2) | NOT NULL | Total booking amount |
| Status | NVARCHAR(50) | NOT NULL | Booking status |
| CreatedAt | DATETIME | NOT NULL | Booking creation timestamp |
| UpdatedAt | DATETIME | NULL | Last status update timestamp |

**Status Values**:
- Pending: Initial booking state
- Confirmed: Payment completed
- CheckedIn: Guest has checked in
- CheckedOut: Guest has checked out
- Completed: Stay completed
- Cancelled: Booking cancelled

**Business Rules**:
- CheckOutDate must be after CheckInDate
- TotalAmount is calculated from BookingDetails
- Status transitions follow a defined workflow

**Relationships**:
- N:1 with `User` (UserId)
- 1:N with `BookingDetail` (room breakdown)
- 1:N with `BookingHistory` (status changes)
- 1:1 with `Payment` (payment transaction)
- 1:N with `BookingVoucher` (applied discounts)
- 1:N with `Transaction` (financial records)

---

### BookingDetail
**Table**: `BookingDetails`  
**Purpose**: Line items for booked rooms (supports multi-room bookings)

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| BookingDetailId | UNIQUEIDENTIFIER | PK | Unique detail identifier |
| BookingId | UNIQUEIDENTIFIER | FK | Reference to Booking |
| RoomTypeId | UNIQUEIDENTIFIER | FK | Reference to RoomType |
| Quantity | INT | NOT NULL | Number of rooms booked |
| Nights | INT | NOT NULL | Number of nights |
| PricePerNight | DECIMAL(18,2) | NOT NULL | Price per night (snapshot) |
| DiscountAmount | DECIMAL(18,2) | DEFAULT 0 | Total discount applied |
| TotalPrice | DECIMAL(18,2) | NOT NULL | Final price for this line item |

**Calculation**:
```
TotalPrice = (PricePerNight × Quantity × Nights) - DiscountAmount
```

**Business Rules**:
- PricePerNight is snapshotted at booking time
- Quantity must not exceed available rooms
- Used for multi-room or multi-type bookings

**Relationships**:
- N:1 with `Booking` (BookingId)
- N:1 with `RoomType` (RoomTypeId)
- 1:N with `Review` (guests review specific booked rooms)

---

### BookingHistory
**Table**: `BookingHistories`  
**Purpose**: Audit trail for booking status changes

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| HistoryId | UNIQUEIDENTIFIER | PK | Unique history record identifier |
| BookingId | UNIQUEIDENTIFIER | FK | Reference to Booking |
| Description | NVARCHAR | NULL | Change description |
| Status | NVARCHAR(50) | NOT NULL | New status after change |
| ChangedAt | DATETIME | NOT NULL | Timestamp of change |
| ChangedBy | UNIQUEIDENTIFIER | NULL | AccountId who made the change |

**Business Rules**:
- Immutable audit trail
- Records every status transition
- Supports compliance and dispute resolution

**Relationships**:
- N:1 with `Booking` (BookingId)
- N:1 with `Account` (ChangedBy)

---

## Payment & Financial

### Payment
**Table**: `Payments`  
**Purpose**: Payment transactions for bookings

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| PaymentId | UNIQUEIDENTIFIER | PK, NOT NULL | Unique payment identifier |
| BookingId | UNIQUEIDENTIFIER | FK | Reference to Booking |
| Amount | DECIMAL(18,2) | NOT NULL | Payment amount |
| PaymentMethod | NVARCHAR(50) | NOT NULL | Payment method used |
| TransactionId | NVARCHAR(200) | | External transaction ID |
| Status | NVARCHAR(50) | NOT NULL | Payment status |
| PaidAt | DATETIME | NULL | Payment completion timestamp |

**Payment Methods**:
- VNPay
- Momo
- CreditCard
- Cash

**Payment Status**:
- Pending: Payment initiated
- Success: Payment completed
- Failed: Payment failed
- Refunded: Payment refunded

**Business Rules**:
- One payment per booking
- PaidAt is set when Status = Success
- TransactionId from payment gateway

**Relationships**:
- 1:1 with `Booking` (BookingId)

---

### Wallet
**Table**: `Wallets`  
**Purpose**: Digital wallet for accounts (Partners, Admin)

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| WalletId | UNIQUEIDENTIFIER | PK | Unique wallet identifier |
| AccountId | UNIQUEIDENTIFIER | FK | Reference to Account |
| Balance | DECIMAL(18,2) | DEFAULT 0 | Current balance |
| CreatedAt | DATETIME | NOT NULL | Wallet creation timestamp |
| UpdatedAt | DATETIME | NULL | Last transaction timestamp |

**Business Rules**:
- Balance cannot be negative
- All balance changes via Transaction records
- Partners receive booking payments here (minus commission)

**Relationships**:
- 1:1 with `Account` (AccountId)
- 1:N with `Transaction` (balance history)
- 1:N with `Settlement` (payout records)

---

### Transaction
**Table**: `Transactions`  
**Purpose**: Financial transaction records for wallet operations

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| TransactionId | UNIQUEIDENTIFIER | PK | Unique transaction identifier |
| WalletId | UNIQUEIDENTIFIER | FK | Reference to Wallet |
| BookingId | UNIQUEIDENTIFIER | NULL, FK | Reference to Booking (if applicable) |
| Amount | DECIMAL(18,2) | NOT NULL | Transaction amount (+ credit, - debit) |
| Type | NVARCHAR(50) | NOT NULL | Transaction type |
| Status | NVARCHAR(50) | NOT NULL | Transaction status |
| CreatedAt | DATETIME | NOT NULL | Transaction timestamp |

**Transaction Types**:
- PAYMENT: Customer payment received
- REFUND: Refund to customer
- SETTLEMENT: Partner payout
- COMMISSION: Platform commission

**Business Rules**:
- Positive amount = credit to wallet
- Negative amount = debit from wallet
- BookingId is NULL for non-booking transactions

**Relationships**:
- N:1 with `Wallet` (WalletId)
- N:1 with `Booking` (BookingId, nullable)

---

### Settlement
**Table**: `Settlements`  
**Purpose**: Partner payout processing records

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| SettlementId | UNIQUEIDENTIFIER | PK | Unique settlement identifier |
| WalletId | UNIQUEIDENTIFIER | FK | Reference to Partner's Wallet |
| PeriodStart | DATETIME | NOT NULL | Settlement period start |
| PeriodEnd | DATETIME | NOT NULL | Settlement period end |
| TotalAmount | DECIMAL(18,2) | NOT NULL | Total amount to partner |
| CommissionAmount | DECIMAL(18,2) | NOT NULL | Platform commission |
| Status | NVARCHAR(50) | NOT NULL | Settlement status |
| PaidAt | DATETIME | NULL | Payout completion timestamp |

**Settlement Status**:
- PENDING: Awaiting processing
- PAID: Settlement completed

**Business Rules**:
- TotalAmount = (Bookings Revenue) - CommissionAmount
- Typically processed weekly/monthly
- PaidAt is set when Status = PAID

**Relationships**:
- N:1 with `Wallet` (WalletId)

---

## Voucher & Promotions

### Voucher
**Table**: `Vouchers`  
**Purpose**: Discount codes for marketing campaigns

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| VoucherId | UNIQUEIDENTIFIER | PK | Unique voucher identifier |
| Code | NVARCHAR(50) | UNIQUE, NOT NULL | Voucher code (e.g., "SUMMER2025") |
| Description | NVARCHAR(255) | | Voucher description |
| DiscountType | NVARCHAR(20) | NOT NULL | PERCENT or AMOUNT |
| DiscountValue | DECIMAL(18,2) | NOT NULL | Discount value (% or amount) |
| MaxDiscountAmount | DECIMAL(18,2) | NULL | Max discount for PERCENT type |
| MinBookingAmount | DECIMAL(18,2) | NULL | Minimum booking requirement |
| UsageLimit | INT | NOT NULL | Total usage limit |
| UsagePerUser | INT | NOT NULL | Per-user usage limit |
| StartDate | DATETIME | NOT NULL | Voucher activation date |
| EndDate | DATETIME | NOT NULL | Voucher expiration date |
| Status | NVARCHAR(50) | NOT NULL | Voucher status |
| CreatedByType | NVARCHAR(20) | NOT NULL | ADMIN or PARTNER |
| CreatedById | UNIQUEIDENTIFIER | NULL, FK | Partner ID if partner-created |
| CreatedAt | DATETIME | NOT NULL | Creation timestamp |
| UpdatedAt | DATETIME | NULL | Last update timestamp |

**Voucher Status**:
- ACTIVE: Can be used
- EXPIRED: Past EndDate
- DISABLED: Manually disabled

**Business Rules**:
- Code must be unique system-wide
- DiscountType = PERCENT: DiscountValue is 0-100
- DiscountType = AMOUNT: DiscountValue is fixed amount
- Can be scoped to specific partners, properties, or rooms via VoucherTarget

**Relationships**:
- 1:N with `VoucherTarget` (scope definition)
- 1:N with `BookingVoucher` (usage tracking)
- N:1 with `Partner` (CreatedById, if partner-created)

---

### VoucherTarget
**Table**: `VoucherTargets`  
**Purpose**: Defines the scope of voucher applicability

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| VoucherTargetId | UNIQUEIDENTIFIER | PK | Unique target identifier |
| VoucherId | UNIQUEIDENTIFIER | FK | Reference to Voucher |
| TargetType | NVARCHAR(20) | NOT NULL | Scope type |
| TargetId | UNIQUEIDENTIFIER | NOT NULL, FK | Scope entity ID |

**Target Types**:
- PARTNER: Voucher applies to all properties of a partner
- PROPERTY: Voucher applies to specific property
- ROOM: Voucher applies to specific room type

**Business Rules**:
- Multiple targets allowed per voucher
- TargetId references PartnerId, PropertyId, or RoomTypeId based on TargetType

**Relationships**:
- N:1 with `Voucher` (VoucherId)
- N:1 with `Partner` (TargetId, when TargetType = PARTNER)
- N:1 with `Property` (TargetId, when TargetType = PROPERTY)
- N:1 with `RoomType` (TargetId, when TargetType = ROOM)

---

### BookingVoucher
**Table**: `BookingVouchers`  
**Purpose**: Tracks voucher usage in bookings

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| BookingVoucherId | UNIQUEIDENTIFIER | PK | Unique record identifier |
| BookingId | UNIQUEIDENTIFIER | FK | Reference to Booking |
| VoucherId | UNIQUEIDENTIFIER | FK | Reference to Voucher |
| AppliedDiscount | DECIMAL(18,2) | NOT NULL | Actual discount applied |

**Business Rules**:
- Records actual discount amount applied
- Used for usage tracking and analytics
- Multiple vouchers can be applied per booking (if business rules allow)

**Relationships**:
- N:1 with `Booking` (BookingId)
- N:1 with `Voucher` (VoucherId)

---

## Review System

### Review
**Table**: `Reviews`  
**Purpose**: Guest reviews for room types (must have stayed)

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| ReviewId | UNIQUEIDENTIFIER | PK | Unique review identifier |
| BookingDetailId | UNIQUEIDENTIFIER | FK | Reference to BookingDetail |
| UserId | UNIQUEIDENTIFIER | FK | Reference to User (reviewer) |
| RoomTypeId | UNIQUEIDENTIFIER | FK | Reference to RoomType |
| Rating | INT | NOT NULL | Rating (1-5 stars) |
| Comment | NVARCHAR(MAX) | | Review text |
| Status | NVARCHAR(50) | NOT NULL | Review status |
| CreatedAt | DATETIME | NOT NULL | Review submission timestamp |
| UpdatedAt | DATETIME | NULL | Last update timestamp |

**Status Values**:
- ACTIVE: Publicly visible
- HIDDEN: Hidden by admin/partner
- DELETED: Soft deleted

**Business Rules**:
- Users can only review rooms they have booked and stayed in
- One review per BookingDetail
- Rating must be 1-5
- Affects RoomType.AverageRating and Property.AverageRating

**Relationships**:
- N:1 with `BookingDetail` (BookingDetailId)
- N:1 with `User` (UserId)
- N:1 with `RoomType` (RoomTypeId)
- 1:N with `ReviewImage` (photo reviews)
- 1:N with `ReviewReply` (responses)

---

### ReviewImage
**Table**: `ReviewImages`  
**Purpose**: Photo attachments for reviews

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| ImageId | UNIQUEIDENTIFIER | PK | Unique image identifier |
| ReviewId | UNIQUEIDENTIFIER | FK | Reference to Review |
| FilePath | NVARCHAR(500) | NOT NULL | Cloud storage path |
| UploadedAt | DATETIME | NOT NULL | Upload timestamp |

**Business Rules**:
- Maximum recommended: 10 images per review
- Images stored in Cloudinary

**Relationships**:
- N:1 with `Review` (ReviewId)

---

### ReviewReply
**Table**: `ReviewReplies`  
**Purpose**: Partner or admin responses to reviews

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| ReplyId | UNIQUEIDENTIFIER | PK | Unique reply identifier |
| ReviewId | UNIQUEIDENTIFIER | FK | Reference to Review |
| ReplierType | NVARCHAR(20) | NOT NULL | PARTNER or ADMIN |
| ReplierId | UNIQUEIDENTIFIER | FK | Reference to Partner or Admin Account |
| Content | NVARCHAR(MAX) | NOT NULL | Reply text |
| CreatedAt | DATETIME | NOT NULL | Reply submission timestamp |
| UpdatedAt | DATETIME | NULL | Last update timestamp |

**Business Rules**:
- Partners can reply to reviews on their properties
- Admins can reply to any review
- One reply per review per replier

**Relationships**:
- N:1 with `Review` (ReviewId)
- N:1 with `Partner` (ReplierId, when ReplierType = PARTNER)
- N:1 with `Account` (ReplierId, when ReplierType = ADMIN)

---

## System Auditing

### AuditLog
**Table**: `AuditLogs`  
**Purpose**: System-wide audit trail for compliance and security

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| AuditLogId | UNIQUEIDENTIFIER | PK | Unique audit log identifier |
| AccountId | UNIQUEIDENTIFIER | FK | Reference to Account (actor) |
| Action | NVARCHAR(200) | NOT NULL | Action performed |
| EntityName | NVARCHAR(100) | NOT NULL | Entity affected |
| EntityId | NVARCHAR(100) | | Entity identifier |
| OldValue | NVARCHAR(MAX) | | JSON snapshot before change |
| NewValue | NVARCHAR(MAX) | | JSON snapshot after change |
| CreatedAt | DATETIME | NOT NULL | Action timestamp |

**Logged Actions**:
- Create, Update, Delete operations
- Status changes
- Permission changes
- Login/logout events

**Relationships**:
- N:1 with `Account` (AccountId)

---

## Relationship Summary

### Foreign Key Relationships

```
Account (1) ──< (N) RefreshToken
Account (1) ──< (N) BlacklistedToken
Account (1) ─── (1) User
Account (1) ─── (1) Partner
Account (1) ─── (1) Wallet
Account (1) ──< (N) AuditLog

Partner (1) ──< (N) PartnerDocument
Partner (1) ──< (N) Property
Partner (1) ──< (N) ReviewReply [when ReplierType = PARTNER]
Partner (1) ──< (N) VoucherTarget [when TargetType = PARTNER]

PropertyType (1) ──< (N) Property

Property (1) ──< (N) PropertyImage
Property (1) ──< (N) RoomType
Property (1) ──< (N) VoucherTarget [when TargetType = PROPERTY]

RoomType (1) ──< (N) RoomTypeImage
RoomType (1) ──< (N) RoomAvailability
RoomType (1) ──< (N) RoomPromotion
RoomType (1) ──< (N) BookingDetail
RoomType (1) ──< (N) Review
RoomType (1) ──< (N) VoucherTarget [when TargetType = ROOM]

User (1) ──< (N) Booking
User (1) ──< (N) Review

Booking (1) ──< (N) BookingDetail
Booking (1) ──< (N) BookingHistory
Booking (1) ─── (1) Payment
Booking (1) ──< (N) BookingVoucher
Booking (1) ──< (N) Transaction

BookingDetail (1) ──< (N) Review

Voucher (1) ──< (N) VoucherTarget
Voucher (1) ──< (N) BookingVoucher

Wallet (1) ──< (N) Transaction
Wallet (1) ──< (N) Settlement

Review (1) ──< (N) ReviewImage
Review (1) ──< (N) ReviewReply
```

---

## Business Rules Summary

### Authentication & Security
1. Refresh tokens expire after 7 days
2. Each refresh token can only be used once
3. Blacklisted access tokens cannot be used for authentication
4. Two-factor authentication via email OTP for login

### User & Partner Management
5. Partners must be approved before listing properties
6. Partner status workflow: Pending → Approved/Rejected
7. Suspended partners cannot receive new bookings
8. Document verification required for partner approval

### Property & Room Management
9. Properties must be approved before appearing in search results
10. Each property must have at least one main image
11. Room types belong to specific properties
12. Room availability managed per date with dynamic pricing
13. Base price can be overridden by RoomAvailability.Price

### Booking & Payment
14. Check-out date must be after check-in date
15. Booking status follows defined workflow (Pending → Confirmed → CheckedIn → CheckedOut → Completed)
16. Payment must be successful before booking confirmation
17. Total amount calculated from BookingDetails
18. Multiple room types can be booked in single reservation

### Promotions & Vouchers
19. Promotions: Either percentage OR fixed amount discount
20. Vouchers must have unique codes system-wide
21. Vouchers can be scoped to specific partners, properties, or rooms
22. Usage limits enforced per voucher and per user
23. Best discount automatically applied (promotion vs voucher)

### Review System
24. Users can only review rooms they have booked and stayed in
25. One review per BookingDetail
26. Rating must be 1-5 stars
27. Reviews affect RoomType and Property average ratings
28. Partners can reply to reviews on their properties

### Financial Management
29. Wallet balance cannot be negative
30. All balance changes tracked via Transaction records
31. Partners receive booking revenue minus platform commission
32. Settlements processed periodically (weekly/monthly)
33. Platform commission deducted from all bookings

---

## Database Constraints & Indexes

### Recommended Indexes
```sql
-- Authentication
CREATE INDEX IX_RefreshTokens_AccountId ON RefreshTokens(AccountId);
CREATE INDEX IX_RefreshTokens_Token ON RefreshTokens(Token);
CREATE INDEX IX_BlacklistedTokens_Token ON BlacklistedTokens(Token);

-- Properties
CREATE INDEX IX_Properties_PartnerId ON Properties(PartnerId);
CREATE INDEX IX_Properties_PropertyTypeId ON Properties(PropertyTypeId);
CREATE INDEX IX_Properties_City_Country ON Properties(City, Country);
CREATE INDEX IX_Properties_Status ON Properties(Status);

-- Room Availability
CREATE INDEX IX_RoomAvailability_RoomTypeId_Date ON RoomAvailabilities(RoomTypeId, Date);

-- Bookings
CREATE INDEX IX_Bookings_UserId ON Bookings(UserId);
CREATE INDEX IX_Bookings_Status ON Bookings(Status);
CREATE INDEX IX_BookingDetails_BookingId ON BookingDetails(BookingId);
CREATE INDEX IX_BookingDetails_RoomTypeId ON BookingDetails(RoomTypeId);

-- Reviews
CREATE INDEX IX_Reviews_RoomTypeId ON Reviews(RoomTypeId);
CREATE INDEX IX_Reviews_UserId ON Reviews(UserId);
CREATE INDEX IX_Reviews_Status ON Reviews(Status);

-- Vouchers
CREATE UNIQUE INDEX IX_Vouchers_Code ON Vouchers(Code);
CREATE INDEX IX_Vouchers_Status_Dates ON Vouchers(Status, StartDate, EndDate);
```

### Check Constraints
```sql
-- Rating validation
ALTER TABLE Reviews ADD CONSTRAINT CK_Reviews_Rating CHECK (Rating >= 1 AND Rating <= 5);

-- Discount validation
ALTER TABLE RoomPromotions ADD CONSTRAINT CK_RoomPromotions_Discount 
    CHECK ((DiscountPercent IS NOT NULL AND DiscountAmount IS NULL) 
        OR (DiscountPercent IS NULL AND DiscountAmount IS NOT NULL));

-- Booking dates validation
ALTER TABLE Bookings ADD CONSTRAINT CK_Bookings_Dates 
    CHECK (CheckOutDate > CheckInDate);

-- Room quantity validation
ALTER TABLE RoomAvailabilities ADD CONSTRAINT CK_RoomAvailabilities_Quantity 
    CHECK (AvailableQuantity >= 0);

-- Wallet balance validation
ALTER TABLE Wallets ADD CONSTRAINT CK_Wallets_Balance 
    CHECK (Balance >= 0);
```

---

## Implementation Status

### ✅ Implemented (Current)
- Account, User, Partner, PartnerDocument
- Wallet
- RefreshToken, BlackListToken
- Property, PropertyType, PropertyImage
- AuditLog
- Booking (basic entity only)

### ❌ Not Yet Implemented (Planned)
- RoomType, RoomTypeImage, RoomAvailability, RoomPromotion
- BookingDetail, BookingHistory, Payment, BookingVoucher
- Review, ReviewImage, ReviewReply
- Voucher, VoucherTarget
- Transaction, Settlement

---

## Migration Strategy

### Phase 1: Room Management (High Priority)
- Create RoomType, RoomTypeImage entities
- Create RoomAvailability for inventory management
- Create RoomPromotion for discounts
- Update Property aggregate to include room relationships

### Phase 2: Enhanced Booking System (High Priority)
- Create BookingDetail for multi-room support
- Create BookingHistory for audit trail
- Create Payment for transaction processing
- Update Booking aggregate relationships

### Phase 3: Financial Enhancement (Medium Priority)
- Create Transaction entity for wallet operations
- Create Settlement for partner payouts
- Update Wallet aggregate with transaction history

### Phase 4: Review System (Medium Priority)
- Create Review entity with validation
- Create ReviewImage for photo reviews
- Create ReviewReply for responses
- Implement rating calculation logic

### Phase 5: Voucher System (Low Priority)
- Create Voucher entity with business rules
- Create VoucherTarget for scoping
- Create BookingVoucher for usage tracking
- Implement discount calculation logic

---

## Conclusion

This ERD represents a comprehensive room booking platform with:
- **23 main entities** covering all business domains
- **Robust authentication** with JWT and token management
- **Multi-tenant architecture** supporting users, partners, and admins
- **Complex booking system** with multi-room support and status workflow
- **Financial management** with wallets, transactions, and settlements
- **Marketing tools** with vouchers and promotions
- **Quality assurance** through reviews and ratings
- **Audit capabilities** for compliance and security

The database design follows **normalization principles**, implements **referential integrity**, and supports **scalable business operations** for the TripEnjoy platform.
