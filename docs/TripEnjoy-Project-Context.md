Let's analyze the flow of how the entire authen feature works: list the functionalities included in this authen feature.# TripEnjoy Project Context

## Business Overview
**TripEnjoy** is a comprehensive room booking platform that connects travelers with accommodation partners. The platform serves as a marketplace enabling property owners/partners to list their accommodations and travelers to discover and book suitable lodging options.

### Core Business Model
- **Platform Type**: Two-sided marketplace (B2B2C)
- **Primary Function**: Room/accommodation booking service
- **Value Proposition**: Connects travelers with diverse accommodation options while providing partners with booking management tools
- **Revenue Model**: Commission-based transactions between guests and accommodation providers

## User Ecosystem

### 1. **Travelers/Guests** (Primary Users)
**Role**: `User`  
**Primary Goals**:
- Search and discover accommodations by destination
- Compare properties, amenities, and pricing
- Make secure bookings for travel dates
- Manage booking history and preferences
- Leave reviews and ratings

**Key User Flows**:
- Account registration and email verification
- Property search by location, dates, and guest count
- Property details review and booking process
- Wallet-based payment system integration
- Post-stay review and rating submission

### 2. **Accommodation Partners** (Supply Side)
**Role**: `Partner`  
**Primary Goals**:
- List and manage multiple properties
- Update availability, pricing, and property details
- Receive and manage booking requests
- Track earnings and payout history
- Maintain property ratings and guest communication

**Partner Onboarding Flow**:
1. **Account Creation**: Register as potential partner
2. **Company Verification**: Submit company details (name, contact, address)
3. **Document Submission**: Upload required business documents
4. **Approval Process**: Admin review and status updates
5. **Property Listing**: Add properties once approved
6. **Go Live**: Accept bookings and manage operations

**Partner Statuses**:
- `Pending`: Initial registration, awaiting verification
- `Approved`: Verified and can list properties
- `Rejected`: Failed verification process
- `Suspended`: Temporarily disabled due to policy violations

### 3. **System Administrators** (Platform Management)
**Role**: `Admin`  
**Primary Responsibilities**:
- Partner application review and approval
- Property content moderation and quality control
- User account management and dispute resolution
- Platform analytics and performance monitoring
- System configuration and maintenance

## Property Ecosystem

### Accommodation Types Supported
TripEnjoy supports diverse property types to cater to different travel needs:

- **Hotel**: Traditional hotel accommodations
- **Apartment**: Short-term apartment rentals
- **Resort**: Full-service resort properties
- **Villa**: Private villa accommodations
- **Cabin**: Rustic/nature-based lodging
- **Guest House**: Small-scale hospitality properties
- **Hostel**: Budget-friendly shared accommodations
- **Motel**: Roadside/transit accommodations

### Property Management Features
- **Property Profiles**: Detailed descriptions, amenities, photos
- **Location Services**: GPS coordinates, address, city/country data
- **Pricing Management**: Dynamic pricing capabilities
- **Availability Calendar**: Real-time booking availability
- **Review System**: Guest rating and feedback collection
- **Image Gallery**: Multiple property photos and virtual tours

### Property Approval Workflow
1. **Partner Submission**: Partners create property listings
2. **Content Review**: Admin validation of property details
3. **Quality Assessment**: Verification of photos and descriptions
4. **Status Updates**: Properties marked as Active/Pending/Rejected
5. **Live Listing**: Approved properties appear in search results

## Technical Architecture Context

### Authentication & Security
- **Two-Factor Authentication**: Email-based OTP verification for secure login
- **Role-Based Access Control**: Granular permissions for User/Partner/Admin roles
- **JWT Token Management**: Secure API authentication with automatic refresh
- **Rate Limiting**: Protected endpoints prevent abuse (5 requests/minute for auth)

### Financial System
- **Wallet Integration**: Each user account includes a digital wallet
- **Transaction Management**: Credit/debit operations with validation
- **Payment Processing**: Secure payment handling for bookings
- **Partner Payouts**: Commission-based earnings distribution
- **Financial Controls**: Insufficient funds validation, transaction limits

### Search & Discovery
- **Location-Based Search**: Find properties by destination
- **Availability Filtering**: Real-time booking calendar integration
- **Guest Capacity**: Search by number of travelers
- **Property Type Filtering**: Filter by accommodation type
- **Rating/Review Integration**: Quality-based property ranking

## Data Flow & Business Processes

### Booking Journey
1. **Discovery**: User searches properties by location/dates
2. **Selection**: Review property details, photos, reviews
3. **Booking**: Select dates, guests, confirm availability
4. **Payment**: Process payment through wallet system
5. **Confirmation**: Generate booking confirmation and notifications
6. **Stay**: Guest experiences accommodation
7. **Review**: Post-stay feedback and ratings

### Partner Revenue Flow
1. **Booking Received**: Guest completes booking at partner property
2. **Payment Processing**: Platform collects payment from guest
3. **Commission Calculation**: Platform fee deducted from booking value
4. **Partner Credit**: Net amount credited to partner wallet
5. **Payout Processing**: Partners can withdraw earnings
6. **Financial Reporting**: Track earnings, bookings, and performance

### Quality Assurance Process
- **Content Moderation**: Review property listings for accuracy
- **Guest Feedback Integration**: Monitor reviews for quality issues
- **Partner Performance Tracking**: Maintain partner quality standards
- **Dispute Resolution**: Handle conflicts between guests and partners

## Competitive Differentiators

### Platform Strengths
- **Diverse Property Types**: Support for 8+ accommodation categories
- **Partner-Centric**: Robust tools for property management and earnings
- **Integrated Wallet System**: Streamlined payment and payout processes
- **Quality Control**: Rigorous approval processes for partners and properties
- **Scalable Architecture**: Clean Architecture with DDD for maintainability

### Target Market Position
- **Mid-Market Focus**: Balance between budget and luxury accommodations
- **Partner Empowerment**: Strong tools for property owners and managers
- **Guest Experience**: Comprehensive search, booking, and review capabilities
- **Regional Expansion**: Architecture supports multi-country/multi-currency operations

## Development Context

### Key Business Rules
- All partners must complete verification before listing properties
- Properties require admin approval before appearing in search results
- Users must have sufficient wallet balance for bookings
- Review system maintains booking authenticity (guests must have stayed)
- Partner earnings calculated after platform commission deduction

### Critical User Stories
1. **As a traveler**, I want to search accommodations by destination so I can find suitable lodging for my trip
2. **As a partner**, I want to list my properties and track bookings so I can manage my hospitality business
3. **As an administrator**, I want to review partner applications so I can maintain platform quality standards
4. **As a guest**, I want to leave reviews after my stay so I can help future travelers make informed decisions
5. **As a partner**, I want to track my earnings and request payouts so I can manage my business finances

### Success Metrics
- **Partner Acquisition**: Number of verified partners and listed properties
- **Booking Volume**: Total bookings processed and revenue generated
- **User Engagement**: Search-to-booking conversion rates
- **Quality Scores**: Average property ratings and guest satisfaction
- **Platform Growth**: Geographic expansion and market penetration

## Domain-Driven Design (DDD) Aggregate Analysis

### Current Implementation Status

Based on the database schema and existing domain structure, TripEnjoy follows DDD principles with the following aggregate boundaries:

#### ✅ **IMPLEMENTED AGGREGATES**

### 1. **Account Aggregate** *(IMPLEMENTED)*
- **Root**: Account
- **Entities**: User, Partner, PartnerDocument, RefreshToken, BlackListToken, Wallet
- **Business Boundary**: User identity, authentication, partner management, and digital wallet functionality
- **Current Status**: ✅ Fully implemented with complete entity relationships
- **Key Features**: Two-factor authentication, partner onboarding workflow, wallet transactions

### 2. **Property Aggregate** *(PARTIALLY IMPLEMENTED)*
- **Root**: Property  
- **Entities**: PropertyImage ✅ | RoomType ❌ | RoomTypeImage ❌ | RoomAvailability ❌ | RoomPromotion ❌
- **Business Boundary**: Property listings, room management, availability, and pricing
- **Current Status**: ⚠️ Basic property management implemented, missing room-level functionality
- **Missing Components**: Room types, availability calendar, promotional pricing

### 3. **PropertyType Aggregate** *(IMPLEMENTED)*
- **Root**: PropertyType
- **Business Boundary**: Property categorization (Hotel, Apartment, Resort, Villa, etc.)
- **Current Status**: ✅ Fully implemented with 8 property types seeded
- **Key Features**: Property classification system with status management

### 4. **AuditLog Aggregate** *(IMPLEMENTED)*
- **Root**: AuditLog
- **Business Boundary**: System change tracking and compliance auditing
- **Current Status**: ✅ Basic audit logging implemented
- **Key Features**: Entity change tracking with old/new value comparison

#### ❌ **MISSING AGGREGATES** (Database Designed but Not Implemented)

### 5. **Booking Aggregate** *(NOT IMPLEMENTED)*
- **Root**: Booking
- **Entities**: BookingDetail, BookingHistory, Payment
- **Business Boundary**: Reservation management, payment processing, booking lifecycle
- **Required Tables**: 
  - Booking (reservations)
  - BookingDetail (room-specific booking items)
  - BookingHistory (status change tracking)
  - Payment (transaction processing)

### 6. **Room Aggregate** *(NOT IMPLEMENTED)*
- **Root**: RoomType
- **Entities**: RoomAvailability, RoomPromotion, RoomTypeImage
- **Business Boundary**: Room inventory, pricing, availability calendar
- **Required Tables**: 
  - RoomType (room categories per property)
  - RoomAvailability (daily availability and pricing)
  - RoomPromotion (discount campaigns)
  - RoomTypeImage (room photos)

### 7. **Review Aggregate** *(NOT IMPLEMENTED)*
- **Root**: Review
- **Entities**: ReviewImage, ReviewReply
- **Business Boundary**: Guest feedback, property ratings, response management
- **Required Tables**: 
  - Review (guest feedback tied to bookings)
  - ReviewImage (photo reviews)
  - ReviewReply (partner/admin responses)

### 8. **Voucher Aggregate** *(NOT IMPLEMENTED)*
- **Root**: Voucher
- **Entities**: VoucherTarget, BookingVoucher
- **Business Boundary**: Promotional campaigns, discount management, usage tracking
- **Required Tables**: 
  - Voucher (discount codes and rules)
  - VoucherTarget (scope: partner/property/room specific)
  - BookingVoucher (applied discounts tracking)

### 9. **Financial Aggregate** *(PARTIALLY IMPLEMENTED)*
- **Root**: Wallet ✅
- **Entities**: Transaction ❌ | Settlement ❌
- **Business Boundary**: Financial operations, partner payouts, commission management
- **Current Status**: ⚠️ Basic wallet implemented, missing transaction history and settlements
- **Missing Components**: Detailed transaction logging, partner payout processing

### **Implementation Roadmap Priority**

Based on business impact and dependencies:

1. **HIGH PRIORITY**: Room Aggregate - Core booking functionality depends on room inventory
2. **HIGH PRIORITY**: Booking Aggregate - Primary business revenue driver
3. **MEDIUM PRIORITY**: Financial Aggregate completion - Transaction tracking and settlements
4. **MEDIUM PRIORITY**: Review Aggregate - Quality assurance and trust building
5. **LOW PRIORITY**: Voucher Aggregate - Marketing and promotional campaigns

### **Domain Architecture Insights**

- **Aggregate Size**: Well-designed aggregate boundaries with appropriate entity groupings
- **Business Invariants**: Clear transactional consistency requirements within aggregates
- **Cross-Aggregate References**: Proper use of ID-based references between aggregates
- **Missing Integration**: Booking process requires coordination between Property, Room, and Account aggregates

This context provides the foundation for understanding TripEnjoy's business domain, user needs, and technical requirements when developing new features or maintaining existing functionality.